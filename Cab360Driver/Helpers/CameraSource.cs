using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Android.Annotation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cab360Driver.FaceDetectorUtils.Common;
using Java.Lang;
using Java.Nio;
using static Android.Hardware.Camera;
using Camera = Android.Hardware.Camera;

namespace Cab360Driver.Helpers
{
    public class CameraSource
    {
        public static CameraFacing cameraFacingBack = CameraFacing.Back;
        public static CameraFacing cameraFacingFront = CameraFacing.Front;
        private static string TAG = "MIDemoApp:CameraSource";
        private static int DUMMY_TEXTURE_NAME = 100;
        private static float ASPECT_RATIO_TOLERANCE = 0.01f;

        protected Activity activity;

        private Android.Hardware.Camera camera;

        protected CameraFacing facing = cameraFacingBack;
        private int rotation;

        private Android.Gms.Common.Images.Size previewSize;
        private float requestedFps = 30.0f;
        private int requestedPreviewWidth = 960;
        private int requestedPreviewHeight = 720;
        private bool requestedAutoFocus = true;
        private SurfaceTexture dummySurfaceTexture;
        private GraphicOverlay graphicOverlay;
        private bool usingSurfaceTexture;
        private Thread processingThread;

        private FrameProcessingRunnable processingRunnable;
        private object processorLock = new object();
        private IVisionImageProcessor frameProcessor;

        private Map<byte[], ByteBuffer> bytesToByteBuffer = new IdentityHashMap<>();

        public CameraSource(Activity activity, GraphicOverlay overlay)
        {
            this.activity = activity;
            graphicOverlay = overlay;
            graphicOverlay.Clear();
            processingRunnable = new FrameProcessingRunnable();
        }

        public void Release()
        {
            lock(processorLock) {
                Stop();
                processingRunnable.Release();
                CleanScreen();

                if (frameProcessor != null)
                {
                    frameProcessor.Stop();
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public CameraSource Start()
        {
            if (camera != null) 
            {
                return this;
            }

            camera = CreateCamera();
            dummySurfaceTexture = new SurfaceTexture(DUMMY_TEXTURE_NAME);
            camera.SetPreviewTexture(dummySurfaceTexture);
            usingSurfaceTexture = true;
            camera.StartPreview();

            processingThread = new Thread(processingRunnable);
            processingRunnable.setActive(true);
            processingThread.Start();
            return this;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            processingRunnable.SetActive(false);
            if (processingThread != null)
            {
                try
                {
                    processingThread.Join();
                }
                catch (InterruptedException e)
                {
                    Log.Debug(TAG, "Frame processing thread interrupted on release.");
                }
                processingThread = null;
            }

            if (camera != null)
            {
                camera.StopPreview();
                camera.SetPreviewCallbackWithBuffer(null);
                try
                {
                    if (usingSurfaceTexture)
                    {
                        camera.SetPreviewTexture(null);
                    }
                    else
                    {
                        camera.SetPreviewDisplay(null);
                    }
                }
                catch (System.Exception e)
                {
                    Log.Error(TAG, "Failed to clear camera preview: " + e);
                }
                camera.Release();
                camera = null;
            }

            // Release the reference to any image buffers, since these will no longer be in use.
            bytesToByteBuffer.Clear();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetFacing(CameraFacing facing)
        {
            if ((facing != cameraFacingBack) && (facing != cameraFacingFront))
            {
                throw new IllegalArgumentException("Invalid camera: " + facing);
            }
            this.facing = facing;
        }

        public Android.Gms.Common.Images.Size GetPreviewSize()
        {
            return previewSize;
        }

        public CameraFacing GetCameraFacing()
        {
            return facing;
        }

        private Camera createCamera()
        {
            int requestedCameraId = GetIdForRequestedCamera(facing);
            if (requestedCameraId == -1) {
                throw new IOException("Could not find requested camera.");
            }
            Camera camera = Camera.Open(requestedCameraId);

            SizePair sizePair = SelectSizePair(camera, requestedPreviewWidth, requestedPreviewHeight);
            if (sizePair == null) 
            {
                throw new IOException("Could not find suitable preview size.");
            }
            Android.Gms.Common.Images.Size pictureSize = sizePair.PictureSize();
            previewSize = sizePair.PreviewSize();

            int[] previewFpsRange = selectPreviewFpsRange(camera, requestedFps);
            if (previewFpsRange == null) 
            {
                throw new IOException("Could not find suitable preview frames per second range.");
            }

            Camera.Parameters parameters = camera.GetParameters();

            if (pictureSize != null) 
            {
              parameters.SetPictureSize(pictureSize.Width, pictureSize.Height);
            }
            parameters.SetPreviewSize(previewSize.Width, previewSize.Height);
            parameters.SetPreviewFpsRange(previewFpsRange[Camera.Parameters.PreviewFpsMaxIndex], previewFpsRange[Camera.Parameters.PreviewFpsMaxIndex]);
            parameters.PreviewFormat = ImageFormatType.Nv21;

            SetRotation(camera, parameters, requestedCameraId);

            if (requestedAutoFocus) 
            {
                if (parameters.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousVideo)) 
                {
                parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousVideo;
                } 
                else {
                Log.Info(TAG, "Camera auto focus is not supported on this device.");
                }
            }

            camera.SetParameters(parameters);

            camera.SetPreviewCallbackWithBuffer(new CameraPreviewCallback());
            camera.AddCallbackBuffer(createPreviewBuffer(previewSize));
            camera.AddCallbackBuffer(createPreviewBuffer(previewSize));
            camera.AddCallbackBuffer(createPreviewBuffer(previewSize));
            camera.AddCallbackBuffer(createPreviewBuffer(previewSize));

            return camera;
        }


        private static int getIdForRequestedCamera(int facing)
        {
            CameraInfo cameraInfo = new CameraInfo();
            for (int i = 0; i < Camera.getNumberOfCameras(); ++i)
            {
                Camera.getCameraInfo(i, cameraInfo);
                if (cameraInfo.facing == facing)
                {
                    return i;
                }
            }
            return -1;
        }

        private static SizePair selectSizePair(Camera camera, int desiredWidth, int desiredHeight)
        {
            List<SizePair> validPreviewSizes = generateValidPreviewSizeList(camera);
            SizePair selectedPair = null;
            int minDiff = Integer.MAX_VALUE;
            foreach (SizePair sizePair in validPreviewSizes)
            {
                Android.Gms.Common.Images.Size size = sizePair.PreviewSize();
                int diff =
                    System.Math.Abs(size.Width - desiredWidth) + System.Math.Abs(size.Height - desiredHeight);
                if (diff < minDiff)
                {
                    selectedPair = sizePair;
                    minDiff = diff;
                }
            }

            return selectedPair;
        }

        private class SizePair
        {
            private Android.Gms.Common.Images.Size preview;
            private Android.Gms.Common.Images.Size picture;

            SizePair(
                Camera.Size previewSize,
                Camera.Size pictureSize)
            {
                preview = new Android.Gms.Common.Images.Size(previewSize.Width, previewSize.Height);
                if (pictureSize != null)
                {
                    picture = new Android.Gms.Common.Images.Size(pictureSize.width, pictureSize.height);
                }
            }

                    Android.Gms.Common.Images.Size previewSize()
            {
                return preview;
            }

            Android.Gms.Common.Images.Size pictureSize()
            {
                return picture;
            }
        }

        private static List<SizePair> generateValidPreviewSizeList(Camera camera)
        {
            Camera.Parameters parameters = camera.GetParameters();
            IList<Camera.Size> supportedPreviewSizes = parameters.SupportedPreviewSizes;
            IList<Camera.Size> supportedPictureSizes = parameters.SupportedPictureSizes;
            List<SizePair> validPreviewSizes = new List<SizePair>();
            foreach (Camera.Size previewSize in supportedPreviewSizes)
            {
                float previewAspectRatio = (float)previewSize.Width / (float)previewSize.Height;

                foreach (Camera.Size pictureSize in supportedPictureSizes)
                {
                    float pictureAspectRatio = (float)pictureSize.Width / (float)pictureSize.Height;
                    if (System.Math.Abs(previewAspectRatio - pictureAspectRatio) < ASPECT_RATIO_TOLERANCE)
                    {
                        validPreviewSizes.Add(new SizePair(previewSize, pictureSize));
                        break;
                    }
                }
            }

            if (validPreviewSizes.Count() == 0)
            {
                Log.Warn(TAG, "No preview sizes have a corresponding same-aspect-ratio picture size");
                foreach (Camera.Size previewSize in supportedPreviewSizes)
                {
                    // The null picture size will let us know that we shouldn't set a picture size.
                    validPreviewSizes.Add(new SizePair(previewSize, null));
                }
            }

            return validPreviewSizes;
        }

        private static int[] selectPreviewFpsRange(Camera camera, float desiredPreviewFps)
        {

            int desiredPreviewFpsScaled = (int)(desiredPreviewFps * 1000.0f);
            int[] selectedFpsRange = null;
            int minDiff = Integer.MAX_VALUE;
            List<int[]> previewFpsRangeList = camera.getParameters().getSupportedPreviewFpsRange();
            for (int[] range : previewFpsRangeList)
            {
                int deltaMin = desiredPreviewFpsScaled - range[Camera.Parameters.PREVIEW_FPS_MIN_INDEX];
                int deltaMax = desiredPreviewFpsScaled - range[Camera.Parameters.PREVIEW_FPS_MAX_INDEX];
                int diff = Math.abs(deltaMin) + Math.abs(deltaMax);
                if (diff < minDiff)
                {
                    selectedFpsRange = range;
                    minDiff = diff;
                }
            }
            return selectedFpsRange;
        }

        private void SetRotation(Camera camera, Camera.Parameters parameters, int cameraId)
        {
            IWindowManager windowManager = (IWindowManager)activity.GetSystemService(Context.WindowService);
            int degrees = 0;
            int rotation = windowManager.getDefaultDisplay().getRotation();
            switch (rotation)
            {
                case Surface.ROTATION_0:
                    degrees = 0;
                    break;
                case Surface.ROTATION_90:
                    degrees = 90;
                    break;
                case Surface.ROTATION_180:
                    degrees = 180;
                    break;
                case Surface.ROTATION_270:
                    degrees = 270;
                    break;
                default:
                    Log.e(TAG, "Bad rotation value: " + rotation);
            }

            CameraInfo cameraInfo = new CameraInfo();
            Camera.getCameraInfo(cameraId, cameraInfo);

            int angle;
            int displayAngle;
            if (cameraInfo.facing == CameraInfo.CAMERA_FACING_FRONT)
            {
                angle = (cameraInfo.Orientation + degrees) % 360;
                displayAngle = (360 - angle) % 360; // compensate for it being mirrored
            }
            else
            { // back-facing
                angle = (cameraInfo.Orientation - degrees + 360) % 360;
                displayAngle = angle;
            }

            // This corresponds to the rotation constants.
            this.rotation = angle / 90;

            camera.SetDisplayOrientation(displayAngle);
            parameters.SetRotation(angle);
        }

        private byte[] createPreviewBuffer(Size previewSize)
        {
            int bitsPerPixel = ImageFormat.getBitsPerPixel(ImageFormat.NV21);
            long sizeInBits = (long)previewSize.getHeight() * previewSize.getWidth() * bitsPerPixel;
            int bufferSize = (int)Math.ceil(sizeInBits / 8.0d) + 1;
            byte[] byteArray = new byte[bufferSize];
            ByteBuffer buffer = ByteBuffer.Wrap(byteArray);
            if (!buffer.HasArray || (buffer.Array != byteArray))
            {
                throw new IllegalStateException("Failed to create valid buffer for camera source.");
            }

            bytesToByteBuffer.Put(byteArray, buffer);
            return byteArray;
        }

        private class CameraPreviewCallback implements Camera.PreviewCallback
        {
            public override void OnPreviewFrame(byte[] data, Camera camera)
            {
                processingRunnable.setNextFrame(data, camera);
            }
    }

        public void SetMachineLearningFrameProcessor(IVisionImageProcessor processor)
        {
            lock(processorLock) {
                CleanScreen();
                if (frameProcessor != null)
                {
                    frameProcessor.Stop();
                }
                frameProcessor = processor;
            }
        }


        private class FrameProcessingRunnable implements Runnable
        {

            // This lock guards all of the member variables below.
            private final Object lock = new Object();
        private boolean active = true;

        // These pending variables hold the state associated with the new frame awaiting processing.
        private ByteBuffer pendingFrameData;

        FrameProcessingRunnable() { }

        /**
         * Releases the underlying receiver. This is only safe to do after the associated thread has
         * completed, which is managed in camera source's release method above.
         */
        @SuppressLint("Assert")
            void release()
        {
            assert(processingThread.getState() == State.TERMINATED);
        }

        /** Marks the runnable as active/not active. Signals any blocked threads to continue. */
        void setActive(boolean active)
        {
            synchronized(lock)
            {
                this.active = active;
                lock.notifyAll();
            }
        }

        /**
         * Sets the frame data received from the camera. This adds the previous unused frame buffer (if
         * present) back to the camera, and keeps a pending reference to the frame data for future use.
         */
        void setNextFrame(byte[] data, Camera camera)
        {
            synchronized(lock)
            {
                if (pendingFrameData != null)
                {
                    camera.addCallbackBuffer(pendingFrameData.array());
                    pendingFrameData = null;
                }

                if (!bytesToByteBuffer.containsKey(data))
                {
                    Log.d(
                        TAG,
                        "Skipping frame. Could not find ByteBuffer associated with the image "
                            + "data from the camera.");
                    return;
                }

                pendingFrameData = bytesToByteBuffer.get(data);

                // Notify the processor thread if it is waiting on the next frame (see below).
                lock.notifyAll();
            }
        }


        public override void Run()
        {
            ByteBuffer data;

            while (true)
            {
                lock(_lock)
                {
                    while (active && (pendingFrameData == null))
                    {
                        try
                        {

                        }
                        catch (InterruptedException e)
                        {
                            Log.Debug(TAG, "Frame processing loop terminated.", e);
                            return;
                        }
                    }

                    if (!active)
                    {
                        return;
                    }
                    data = pendingFrameData;
                    pendingFrameData = null;
                }
                try
                {
                    lock(processorLock) {
                        Log.d(TAG, "Process an image");
                        frameProcessor.process(
                            data,
                            new FrameMetadata.Builder()
                                .setWidth(previewSize.getWidth())
                                .setHeight(previewSize.getHeight())
                                .setRotation(rotation)
                                .setCameraFacing(facing)
                                .build(),
                            graphicOverlay);
                    }
                }
                catch (Throwable t)
                {
                    Log.Error(TAG, "Exception thrown from receiver.", t);
                }
                finally
                {
                    camera.AddCallbackBuffer(data.Array);
                }
            }
        }
          }

          /** Cleans up graphicOverlay and child classes can do their cleanups as well . */
        private void CleanScreen()
        {
            GraphicOverlay.Clear();
        }
    }
}