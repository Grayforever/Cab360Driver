using Android.Graphics;
using Java.Nio;

namespace Cab360Driver.FaceDetectorUtils.Common
{
    public interface IVisionImageProcessor
    {
        void Process(ByteBuffer byteBuffer, FrameMetadata frameMetadata, GraphicOverlay graphicOverlay);
        void Process(Bitmap bitmap, GraphicOverlay graphicOverlay);
        void Stop();
    }
}