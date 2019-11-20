using System;
using System.Buffers;
using System.Drawing;
using Microsoft.Azure.Kinect.Sensor;

namespace AzureKinectSensorConnector.Extensions
{
    public static class ImagesExtensions
    {
        public static Bitmap CreateBitmap(this Microsoft.Azure.Kinect.Sensor.Image image)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            System.Drawing.Imaging.PixelFormat pixelFormat;

            using (Microsoft.Azure.Kinect.Sensor.Image reference = image.Reference())
            {
                unsafe
                {
                    switch (reference.Format)
                    {
                        case ImageFormat.ColorBGRA32:
                            pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                            break;
                        case ImageFormat.Depth16:
                        case ImageFormat.IR16:
                            pixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
                            break;
                        default:
                            throw new AzureKinectException($"Pixel format {reference.Format} cannot be converted to a BitmapSource");
                    }

                    using (MemoryHandle pin = image.Memory.Pin())
                    {
                        return new Bitmap(
                            image.WidthPixels,
                            image.HeightPixels,
                            image.StrideBytes,
                            pixelFormat,
                            (IntPtr)pin.Pointer);
                    }
                }
            }
        }
    }
}
