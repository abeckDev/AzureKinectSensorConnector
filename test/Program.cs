using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Kinect.Sensor;
using AbeckDev.AzureKinectSensorConnector.Extensions;

namespace AbeckDev.AzureKinectSensorConnector.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Auto dispose sensor after we wont need him
            using (Device kinect = Device.Open(0))
            {
                //Get sample information from the sensor
                Console.WriteLine("Device Serial Number: " + kinect.SerialNum);

                //Configure Cameras 
                kinect.StartCameras(new DeviceConfiguration
                {
                    //Only ColorBGRA32, Depth16 and IR16 can be transformed to a .bmp file
                    ColorFormat = ImageFormat.ColorBGRA32,
                    ColorResolution = ColorResolution.R1080p,
                    DepthMode = DepthMode.NFOV_2x2Binned,
                    SynchronizedImagesOnly = true,
                    CameraFPS = FPS.FPS30
                });

                //Create a sample RGB bitmap and save it to the local disk
                var bitmap = kinect.CreateRgbColorBitmapAsync().GetAwaiter().GetResult();
                bitmap.Save(@"C:\temp\kinect_rgbBitmap.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                
                //Create a sample Depth bitmap and save it to the local disk
                var depthbitmap = kinect.CreateDepthColorBitmapAsync().GetAwaiter().GetResult();
                depthbitmap.Save(@"C:\temp\kinect_deptBitmap.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            }
            //Tell the user that we are done
            Console.WriteLine("Done. Sensor will be disposed.");

#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
