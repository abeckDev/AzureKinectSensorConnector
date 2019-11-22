using AbeckDev.AzureKinectSensorConnector.Exceptions;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbeckDev.AzureKinectSensorConnector.Extensions
{
    public static class DeviceExtensions
    {

        /// <summary>
        /// This will capture a Color Image and save it as <see cref="Bitmap"/>
        /// </summary>
        /// <param name="kinectSensor">The initialized Kinect Sensor object</param>
        /// <returns>Returns the Picture from the Color Camera as <see cref="Bitmap"/></returns>
        public static async Task<Bitmap> CreateRgbColorBitmapAsync(this Device kinectSensor)
        {
            try
            {
              

                //Check for initialized Camera
                try
                {
                    var Calibration = kinectSensor.GetCalibration().ColorCameraCalibration.ResolutionWidth;
                }
                catch (Exception e)
                {
                    //Camera not initialized 
                    throw new CameraNotInitializedException("The Camera was not initialized correctly.", e);
                }
                
                // Wait for a capture on a thread pool thread
                using (Capture capture = await Task.Run(() => { return kinectSensor.GetCapture(); }).ConfigureAwait(true))
                {
                    // Create a BitmapSource for the unmodified color image.
                    // Creating the BitmapSource is slow, so do it asynchronously on another thread
                    Task<System.Drawing.Bitmap> createRgbColorBitmapTask = Task.Run(() =>
                    {
                        return capture.Color.CreateBitmap();
                    });

                    // Wait for  bitmap and return it
                    var rgbColorBitmap = await createRgbColorBitmapTask.ConfigureAwait(true);
                    return rgbColorBitmap;
                }          
            }
            //In case something goes wrong...
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// This will capture a Depth enabled Color Image and save it as <see cref="Bitmap"/>
        /// Picture will be colorized with Red where there is no depth data, and Green
        /// where there is depth data at more than 1.5 meters
        /// </summary>
        /// <param name="kinectSensor">The initialized Kinect Sensor object</param>
        /// <returns>Returns the Picture from the Color Camera as <see cref="Bitmap"/></returns>
        public static async Task<Bitmap> CreateDepthColorBitmapAsync(this Device kinectSensor)
        {
            try
            {
                //Declare calibration settings
                int colorWidth;
                int colorHeight;

                //Check for initialized Camera
                try
                {
                    colorWidth = kinectSensor.GetCalibration().ColorCameraCalibration.ResolutionWidth;
                    colorHeight = kinectSensor.GetCalibration().ColorCameraCalibration.ResolutionHeight;
                }
                catch (Exception e)
                {
                    //Camera not initialized 
                    throw new CameraNotInitializedException("The Camera was not initialized correctly.", e);
                }

                // Configure transformation module in order to transform depth enabled picture as bitmap
                using (Microsoft.Azure.Kinect.Sensor.Image transformedDepth = new Microsoft.Azure.Kinect.Sensor.Image(ImageFormat.Depth16, colorWidth, colorHeight))
                using (Microsoft.Azure.Kinect.Sensor.Image outputColorImage = new Microsoft.Azure.Kinect.Sensor.Image(ImageFormat.ColorBGRA32, colorWidth, colorHeight))
                using (Transformation transform = kinectSensor.GetCalibration().CreateTransformation())
                {
                    // Wait for a capture on a thread pool thread
                    using (Capture capture = await Task.Run(() => { return kinectSensor.GetCapture(); }).ConfigureAwait(true))
                    {
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

                        // Wait for  bitmap and return 
                        var depthColorBitmap = await createDepthColorBitmapTask.ConfigureAwait(true);
                        return depthColorBitmap;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
