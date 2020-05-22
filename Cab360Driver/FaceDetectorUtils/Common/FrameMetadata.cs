using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Cab360Driver.FaceDetectorUtils.Common
{
    public class FrameMetadata
    {
        private readonly int width;
        private readonly int height;
        private readonly int rotation;
        private readonly int cameraFacing;

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public int GetRotation()
        {
            return rotation;

        }

        public int GetCameraFacing()
        {
            return cameraFacing;
        }
    }
}