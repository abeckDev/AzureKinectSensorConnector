using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AzureKinectSensorConnector.Extensions;
using Microsoft.Azure.Kinect;
using Microsoft.Azure.Kinect.Sensor;

namespace AzureKinectSensorConnector
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Trying to open device");
            try
            {
                using (Device kinect = Device.Open(0))
                {
                    Console.WriteLine("Device Serial Number: " + kinect.SerialNum);
                    kinect.StartCameras(new DeviceConfiguration
                    {
                        ColorFormat = ImageFormat.ColorBGRA32,
                        ColorResolution = ColorResolution.R1080p,
                        DepthMode = DepthMode.NFOV_2x2Binned,
                        SynchronizedImagesOnly = true,
                        CameraFPS = FPS.FPS30
                    });

                    int colorWidth = kinect.GetCalibration().ColorCameraCalibration.ResolutionWidth;
                    int colorHeight = kinect.GetCalibration().ColorCameraCalibration.ResolutionHeight;

                    int count = 1;
                    while (count <= 5)
                    {

                        Console.WriteLine("I will now take picture " + count.ToString());

                        // Start Picture loop
                        using (Image transformedDepth = new Image(ImageFormat.Depth16, colorWidth, colorHeight))
                        using (Image outputColorImage = new Image(ImageFormat.ColorBGRA32, colorWidth, colorHeight))
                        using (Transformation transform = kinect.GetCalibration().CreateTransformation())
                        {
                            // Wait for a capture on a thread pool thread
                            using (Capture capture = await Task.Run(() => { return kinect.GetCapture(); }).ConfigureAwait(true))
                            {
                                // Create a BitmapSource for the unmodified color image.
                                // Creating the BitmapSource is slow, so do it asynchronously on another thread
                                Task<System.Drawing.Bitmap> createRgbColorBitmapTask = Task.Run(() =>
                                {
                                    return capture.Color.CreateBitmap();
                                });

                                // Compute the colorized output bitmap on a thread pool thread
                                Task<System.Drawing.Bitmap> createDepthColorBitmapTask = Task.Run(() =>
                                {
                                // Transform the depth image to the perspective of the color camera
                                transform.DepthImageToColorCamera(capture, transformedDepth);

                                // Get Span<T> references to the pixel buffers for fast pixel access.
                                Span<ushort> depthBuffer = transformedDepth.GetPixels<ushort>().Span;
                                    Span<BGRA> colorBuffer = capture.Color.GetPixels<BGRA>().Span;
                                    Span<BGRA> outputColorBuffer = outputColorImage.GetPixels<BGRA>().Span;

                                // Create an output color image with data from the depth image
                                for (int i = 0; i < colorBuffer.Length; i++)
                                    {
                                    // The output image will be the same as the input color image,
                                    // but colorized with Red where there is no depth data, and Green
                                    // where there is depth data at more than 1.5 meters
                                    outputColorBuffer[i] = colorBuffer[i];
                                        if (depthBuffer[i] == 0)
                                        {
                                            outputColorBuffer[i].R = 255;
                                        }
                                        else if (depthBuffer[i] > 1500)
                                        {
                                            outputColorBuffer[i].G = 255;
                                        }
                                    }

                                    return outputColorImage.CreateBitmap();
                                });

                                // Wait for both bitmaps to be ready and assign them.
                                var rgbColorBitmap = await createRgbColorBitmapTask.ConfigureAwait(true);
                                var depthColorBitmap = await createDepthColorBitmapTask.ConfigureAwait(true);

                                rgbColorBitmap.Save(@"C:\temp\kinect_rgb-"+ count + @".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                                depthColorBitmap.Save(@"C:\temp\kinect_depth-"+ count + @".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                            }
                        }
                        //End Picture Mode

                        //Start sending Picture 

                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("Prediction-Key", "e5e315ee728947668beb1824b3412db3");
                            string url = @"https://southcentralus.api.cognitive.microsoft.com/customvision/v3.0/Prediction/29889410-de7f-47f7-b7de-7a519c3afe54/classify/iterations/Iteration4/image";

                            HttpResponseMessage response;

                            byte[] byteData = GetImageAsByteArray(@"C:\temp\kinect_rgb-" + count + @".bmp");

                            using (var content = new ByteArrayContent(byteData))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                                response = await client.PostAsync(url, content);
                                Console.WriteLine(await response.Content.ReadAsStringAsync());
                            }
                        }

                        System.Threading.Thread.Sleep(1000);
                        count++;

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong: " + e.Message);
            }

            Console.WriteLine("Finished");
            Console.WriteLine("Press a key to exit...");
            Console.ReadLine();
        }

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
    }
}
