# AzureKinectSensorConnector
![GitHub](https://img.shields.io/github/license/abeckDev/AzureKinectSensorConnector)
![Nuget](https://img.shields.io/nuget/dt/AbeckDev.AzureKinectSensorConnector)

A .NET Library which captures RgbColor and DepthColor Images from the Azure Kinect Sensor and returns them as Bitmaps.

## Getting Started

The Lib is available as NuGet Package. Follow the instructions below to use it in your projects.

### Prerequisites

The project lib is currently written in .NET 4.8 (Full Framework) during some limitations in the System.Drawing Lib of .NET Core.
In order to work with the Lib you need to the Full Framework instead of .NET Core for now.

### Installing

To get started add the Package and the Kinect Sensor SDK to to your Project with NuGet:

```
Install-Package AbeckDev.AzureKinectSensorConnector
Install-Package Microsoft.Azure.Kinect.Sensor
```
To access the Azure Kinect Sonsor Connector you need to initialize the Kinect Sensor ```Device``` object and start the cameras. 
This can be done like in the code below:

```csharp
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
                ...
             }
```

Please make sure that you configure the ColorFormat as describe in the sample below. 
Not all available image formats can be used with the lib. 
Supported are:

  * ColorBGRA32
  * Depth16 
  * IR16

After you started the cameras you can capture a Bitmap as shown in the code below:

```csharp

                //Create a sample RGB bitmap and save it to the local disk
                var bitmap = kinect.CreateRgbColorBitmapAsync().GetAwaiter().GetResult();
                bitmap.Save(@"C:\temp\kinect_rgbBitmap.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                
                //Create a sample Depth bitmap and save it to the local disk
                var depthbitmap = kinect.CreateDepthColorBitmapAsync().GetAwaiter().GetResult();
                depthbitmap.Save(@"C:\temp\kinect_deptBitmap.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
```


## Built With

* [Microsoft .NET Framework](https://dotnet.microsoft.com/) - The framework used
* [NuGet](https://www.nuget.org/) - Packaging Tool

## Contributing

Contributing instructions will be added soon!

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Alexander Beck** - *Initial work* - [abeckDev](https://github.com/abeckdev)
* **Kirsten Kluge** - *Helped to understand the principles behind Bitmaps and made this project possible* - [kirkone](https://github.com/kirkone)
* **Nadja Klein** - *Initial work* - [nadjaklein](https://github.com/nadjaklein)
* **Joseph OLeary** - *Provided the hardware and supported during the initial work* - [josephwoleary](https://github.com/josephwoleary)

See also the list of [contributors](https://github.com/abeckDev/AzureKinectSensorConnector/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## Acknowledgments

* Thank you [Microsoft Kinect Samples](https://github.com/microsoft/Azure-Kinect-Samples) for providing us samples on how to use the Kinect SDK!
