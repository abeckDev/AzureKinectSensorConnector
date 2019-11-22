using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbeckDev.AzureKinectSensorConnector.Exceptions
{

    [Serializable]
    public class CameraNotInitializedException : Exception
    {
        public CameraNotInitializedException() { }
        public CameraNotInitializedException(string message) : base(message) { }
        public CameraNotInitializedException(string message, Exception inner) : base(message, inner) { }
        protected CameraNotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
