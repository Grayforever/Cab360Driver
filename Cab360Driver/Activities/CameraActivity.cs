using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Cab360Driver.Helpers;
using Firebase.ML.Vision;
using Firebase.ML.Vision.Common;
using Firebase.ML.Vision.Face;
using Google.Android.Material.FloatingActionButton;
using IO.FotoapparatLib;
using IO.FotoapparatLib.Configurations;
using IO.FotoapparatLib.Errors;
using IO.FotoapparatLib.Exceptions.Cameras;
using IO.FotoapparatLib.Parameters;
using IO.FotoapparatLib.Preview;
using IO.FotoapparatLib.Results;
using IO.FotoapparatLib.Results.Transformers;
using IO.FotoapparatLib.Views;
using Java.IO;
using System;
using static IO.FotoapparatLib.Logs.LoggersKt;
using static IO.FotoapparatLib.Selectors.AspectRatioSelectorsKt;
using static IO.FotoapparatLib.Selectors.FlashSelectorsKt;
using static IO.FotoapparatLib.Selectors.FocusModeSelectorsKt;
using static IO.FotoapparatLib.Selectors.LensPositionSelectorsKt;
using static IO.FotoapparatLib.Selectors.PreviewFpsRangeSelectorsKt;
using static IO.FotoapparatLib.Selectors.ResolutionSelectorsKt;
using static IO.FotoapparatLib.Selectors.SelectorsKt;
using static IO.FotoapparatLib.Selectors.SensorSensitivitySelectorsKt;

namespace Cab360Driver.Activities
{
    [Activity(Label = "Camera")]
    public class CameraActivity : AppCompatActivity
    {
        private CameraView cameraView;
        private FocusView focusView;
        private Fotoapparat fotoapparat;
        private FloatingActionButton capture;
        private FloatingActionButton switchCamera;
        public event EventHandler<ImageCapturedEventArgs> onImageCaptured;

        bool activeCameraBack = true;

        private CameraConfiguration cameraConfiguration = new CameraConfiguration.Builder()
            .PhotoResolution(StandardRatio(HighestResolution()))
            .FocusMode(FirstAvailable(ContinuousFocusPicture(), AutoFocus(), Fixed()))
            .Flash(FirstAvailable(AutoFlash(), Torch(), AutoRedEye()))
            .PreviewFpsRange(HighestFps())
            .SensorSensitivity(HighestSensorSensitivity())
            .FrameProcessor(new SampleFrameProcessor())
            .Build();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.cam_layout);
            cameraView = FindViewById<CameraView>(Resource.Id.cameraView);
            focusView = FindViewById<FocusView>(Resource.Id.focusView);
            capture = FindViewById<FloatingActionButton>(Resource.Id.fabCapture);
            switchCamera = FindViewById<FloatingActionButton>(Resource.Id.fabSwitchCam);
            fotoapparat = CreateFotoapparat();

            TakePictureOnClick();
            SwitchCameraOnClick();

            // Create your application here
        }

        private Fotoapparat CreateFotoapparat()
        {
            return Fotoapparat
                .With(this)
                .Into(cameraView)
                .FocusView(focusView)
                .PreviewScaleType(ScaleType.CenterCrop)
                .LensPosition(Front())
                .FrameProcessor(new SampleFrameProcessor())
                .Logger(Loggers(Logcat(), FileLogger(this)))
                .CameraErrorCallback(new CameraErrorListener(e =>
                {
                    Toast.MakeText(this, e.Message, ToastLength.Long).Show();
                })).Build();
        }

        private void SwitchCameraOnClick()
        {
            bool hasFrontCamera = fotoapparat.IsAvailable(Front());
            switchCamera.Visibility = hasFrontCamera ? ViewStates.Visible : ViewStates.Gone;
            if (hasFrontCamera)
            {
                SwitchCameraOnClick(switchCamera);
            }
        }

        private void SwitchCameraOnClick(FloatingActionButton switchCamera)
        {
            switchCamera.Click += (s1, e1) =>
            {
                activeCameraBack = !activeCameraBack;
                fotoapparat.SwitchTo(activeCameraBack ? Back() : Front(), cameraConfiguration);
            };
        }

        private void TakePictureOnClick()
        {
            capture.Click += (s1, e1) =>
            {
                TakePicture();
            };
        }

        private void TakePicture()
        {
            PhotoResult photoResult = fotoapparat.TakePicture();
            photoResult.SaveToFile(new File(GetExternalFilesDir("photos"), "photo.jpg"));
            photoResult
                .ToBitmap(ResolutionTransformersKt.Scaled(0.25f))
                .WhenDone(new WhenDoneListener(r =>
                {
                    if (r == null)
                    {
                        return;
                    }

                    onImageCaptured?.Invoke(this, new ImageCapturedEventArgs { image = r.JavaCast<BitmapPhoto>().Bitmap });
                    GC.Collect();
                    Finish();
                }));
        }

        private void StartDetection(Bitmap bitmap)
        {
            FirebaseVisionFaceDetectorOptions options = new FirebaseVisionFaceDetectorOptions.Builder()
                .SetClassificationType(FirebaseVisionFaceDetectorOptions.AllClassifications)
                .SetLandmarkType(FirebaseVisionFaceDetectorOptions.AllLandmarks)
                .SetModeType(FirebaseVisionFaceDetectorOptions.AccurateMode)
                .Build();

            FirebaseVisionFaceDetector detector = FirebaseVision.Instance.GetVisionFaceDetector(options);
            FirebaseVisionImage image = FirebaseVisionImage.FromBitmap(bitmap);
            detector.DetectInImage(image).AddOnSuccessListener(new OnSuccessListener(r => 
            {
                JavaList<FirebaseVisionFace> faces = r.JavaCast<JavaList<FirebaseVisionFace>>();
            })).AddOnFailureListener(new OnFailureListener(f=> { Toast.MakeText(this, f.Message, ToastLength.Long).Show(); }));
        }

        private void ProcessResult(JavaList<FirebaseVisionFace> faces)
        {
            float smileProb = 0;
            float leftEyeOpenProb = 0;
            float rightEyeOpenProb = 0;

            foreach(var face in faces)
            {
                if (face.SmilingProbability != FirebaseVisionFace.UncomputedProbability)
                {
                    smileProb = face.SmilingProbability;
                }
                if (face.LeftEyeOpenProbability != FirebaseVisionFace.UncomputedProbability)
                {
                    leftEyeOpenProb = face.LeftEyeOpenProbability;
                }
                if (face.RightEyeOpenProbability != FirebaseVisionFace.UncomputedProbability)
                {
                    rightEyeOpenProb = face.RightEyeOpenProbability;
                }

                Toast.MakeText(this, "face found", ToastLength.Long).Show();
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            fotoapparat.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
            fotoapparat.Stop();
        }

        internal sealed class SampleFrameProcessor : Java.Lang.Object, IFrameProcessor
        {
            public void Process(Frame p0)
            {
                
            }
        }

        internal sealed class CameraErrorListener : Java.Lang.Object, ICameraErrorListener
        {
            private Action<CameraException> _onError;

            public CameraErrorListener(Action<CameraException> onError)
            {
                _onError = onError;
            }

            public void OnError(CameraException p0)
            {
                _onError?.Invoke(p0);
            }
        }

        internal sealed class WhenDoneListener : Java.Lang.Object, IWhenDoneListener
        {
            private Action<Java.Lang.Object> _whenDone;
            public WhenDoneListener(Action<Java.Lang.Object> whenDone)
            {
                _whenDone = whenDone;
            }
            public void WhenDone(Java.Lang.Object p0)
            {
                _whenDone?.Invoke(p0);
            }
        }

        public class ImageCapturedEventArgs : EventArgs
        {
            public Bitmap image { get; set; }
        }
    }
}