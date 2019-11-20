using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

                    // Start Picture loop
                    using (Image transformedDepth = new Image(ImageFormat.Depth16, colorWidth, colorHeight))
                    using (Image outputColorImage = new Image(ImageFormat.ColorBGRA32, colorWidth, colorHeight))
                    using (Transformation transform = kinect.GetCalibration().CreateTransformation())
                    {
                        // Wait for a capture on a thread pool thread
                        using (Capture capture = await Task.Run(() => { return kinect.GetCapture(); }).ConfigureAwait(true))
                        {
                            Console.WriteLine("Temperatur: " + capture.Temperature);

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

                            rgbColorBitmap.Save(@"C:\temp\kinect_rgb.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                            depthColorBitmap.Save(@"C:\temp\kinect_depth.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                        }
                    }
                    //End Picture Mode

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("MIST: " + e.Message);
            }

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
