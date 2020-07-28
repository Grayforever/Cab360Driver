using Android;
using Android.Content;
using Android.Content.Res;
using Android.Gms.Tasks;
using Android.Gms.Vision;
using Android.Gms.Vision.Faces;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using Cab360Driver.Activities;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Firebase.Storage;
using System;
using System.IO;

namespace Cab360Driver.Fragments
{
    public class DriverCaptureFragment : AndroidX.Fragment.App.Fragment
    {
        private CardView Card1, Card2, Card3;
        private TextView WelcomeTxt;
        private ImageView NxtImg1, NxtImg2, NxtImg3;
        private TextView HeaderTxt1; 
        private TextView HeaderTxt2;
        private TextView HeaderTxt3;
        public event EventHandler ProfileCaptured;
        public const int RequestPermission = 200;
        public static StorageReference imageRef;
        private Bitmap bitmap;
        private PicDisplayFragment picDisplayFragment;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(new string[]
                {
                    Manifest.Permission.Camera
                }, RequestPermission);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.driver_capture_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            NxtImg1 = view.FindViewById<ImageView>(Resource.Id.nxt_img1);
            NxtImg2 = view.FindViewById<ImageView>(Resource.Id.nxt_img2);
            NxtImg3 = view.FindViewById<ImageView>(Resource.Id.nxt_img3);
            HeaderTxt1 = view.FindViewById<TextView>(Resource.Id.rec_text);
            HeaderTxt2 = view.FindViewById<TextView>(Resource.Id.rec_text2);
            HeaderTxt3 = view.FindViewById<TextView>(Resource.Id.rec_text3);
            Card1 = view.FindViewById<CardView>(Resource.Id.req_c1);
            Card2 = view.FindViewById<CardView>(Resource.Id.req_c2);
            Card3 = view.FindViewById<CardView>(Resource.Id.req_c3);
            WelcomeTxt = view.FindViewById<TextView>(Resource.Id.drv_capt_welc_tv);

            Card1.Click += Card1_Click;
            Card2.Click += Card2_Click;
            Card3.Click += Card3_Click;
        }

        private void Card3_Click(object sender, EventArgs e)
        {
            BeginCameraInvoke(CaptureType.BackOfLicense);
        }

        private void Card2_Click(object sender, EventArgs e)
        {
            BeginCameraInvoke(CaptureType.FrontOfLicense);
        }

        private void Card1_Click(object sender, EventArgs e)
        {
            BeginCameraInvoke(CaptureType.ProfilePic);
        }

        private void BeginCameraInvoke(CaptureType captureType)
        {
            switch (captureType)
            {
                case CaptureType.ProfilePic:
                    BeforeUSnapFragment bUSnap = new BeforeUSnapFragment();
                    AndroidX.Fragment.App.FragmentTransaction ft = ChildFragmentManager.BeginTransaction();
                    ft.Add(bUSnap, "camera_intro");
                    ft.CommitAllowingStateLoss();
                    bUSnap.StartCameraAsync += CameraIntroDialog_StartCameraAsync;
                    break;
                case CaptureType.FrontOfLicense:
                    break;
                case CaptureType.BackOfLicense:
                    break;
            }
        }

        private void CameraIntroDialog_StartCameraAsync(object sender, EventArgs args)
        {
            OnboardingActivity.ShowProgressDialog();
            CheckAndStartCamera();
        }

        private void CheckAndStartCamera()
        {
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
            {
                var cameraIntent = new Intent(Activity, typeof(CameraActivity));
                StartActivity(cameraIntent);
                CameraActivity.onImageCaptured += CameraActivity_onImageCaptured;
            }
            else
            {
                ShouldShowRequestPermissionRationale(Manifest.Permission.Camera);
            }
        }

        private void CameraActivity_onImageCaptured(object sender, CameraActivity.ImageCapturedEventArgs e)
        {
            OnboardingActivity.CloseProgressDialog();
            bitmap = e.ProfilePic;
            DisplayPic(e.RotationDegrees);
        }

        private void DisplayPic(int rotation)
        {
            picDisplayFragment = new PicDisplayFragment(bitmap, rotation);
            picDisplayFragment.Cancelable = true;
            AndroidX.Fragment.App.FragmentTransaction ft = ChildFragmentManager.BeginTransaction();
            ft.Add(picDisplayFragment, "pic_display");
            ft.CommitAllowingStateLoss();
            picDisplayFragment.SavePic += PicDisplayFragment_SavePic;
            picDisplayFragment.RetakePic += PicDisplayFragment_RetakePic;
        }

        private void PicDisplayFragment_SavePic(object sender, EventArgs e)
        {
            //DetectFace();
            SaveImage();
        }

        private void DetectFace()
        {
            OnboardingActivity.ShowProgressDialog();
            try
            {
                FaceDetector detector = new FaceDetector.Builder(Context)
                    .SetMode(FaceDetectionMode.Accurate)
                    .SetClassificationType(ClassificationType.All)
                    .SetLandmarkType(LandmarkDetectionType.All)
                    .SetProminentFaceOnly(true)
                    .SetTrackingEnabled(false)
                    .Build();

                if (!detector.IsOperational)
                {
                    //Handle contingency
                }
                else
                {
                    Frame frame = new Frame.Builder()
                        .SetBitmap(bitmap)
                        .Build();

                    var faces = detector.Detect(frame);
                    detector.Release();
                    DetectedResponse(bitmap, faces);
                }

                
                SaveImage();
            }
            catch (Exception e)
            {
                OnboardingActivity.CloseProgressDialog();
                OnboardingActivity.ShowErrorDialog(e.Message);
            }
        }

        private void DetectedResponse(Bitmap bitmap, SparseArray faces)
        {
            if (faces.Size() < 1)
            {
                OnboardingActivity.CloseProgressDialog();
                OnboardingActivity.ShowErrorDialog("No face in photo");
            }
            else if (faces.Size() == 1)
            {
                SaveImage();
            }
            else if (faces.Size() > 1)
            {
                OnboardingActivity.CloseProgressDialog();
                OnboardingActivity.ShowErrorDialog("More than one face detected");
            }
        }

        private void SaveImage()
        {
            picDisplayFragment.DismissAllowingStateLoss();
            var storeRef = FirebaseStorage.Instance.GetReferenceFromUrl("gs://taxiproject-185a4.appspot.com");
            if (AppDataHelper.GetCurrentUser() != null)
            {
                var stream = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Webp, 80, stream);
                byte[] bitmapArray = stream.ToArray();

                imageRef = storeRef.Child("driverProfilePics/" + AppDataHelper.GetCurrentUser().Uid);
                imageRef.PutBytes(bitmapArray).ContinueWithTask(new ContinuationTask(t =>
                {
                    if (!t.IsSuccessful)
                    {
                        OnboardingActivity.CloseProgressDialog();
                        Toast.MakeText(Activity, t.Exception.Message, ToastLength.Long).Show();
                    }

                })).AddOnCompleteListener(new OnCompleteListener(t =>
                {
                    if (t.IsSuccessful)
                    {
                        var driverRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}");
                        driverRef.Child("profile_img_url").SetValue(t.Result.ToString()).AddOnCompleteListener(new OnCompleteListener(t3 =>
                        {
                            if (!t3.IsSuccessful)
                                Toast.MakeText(Activity, t3.Exception.Message, ToastLength.Long).Show();
                            OnboardingActivity.CloseProgressDialog();

                            driverRef.Child(StringConstants.StageofRegistration).SetValue($"{RegistrationStage.CarRegistering}")
                            .AddOnSuccessListener(new OnSuccessListener(r2 =>
                            {
                                ProfileCaptured.Invoke(this, new EventArgs());
                                UpdateUiOnCpture(CaptureType.ProfilePic);
                                OnboardingActivity.CloseProgressDialog();

                            })).AddOnFailureListener(new OnFailureListener(e2 =>
                            {
                                OnboardingActivity.CloseProgressDialog();
                                OnboardingActivity.ShowErrorDialog("Something went wrong, please retry");

                            }));
                        }));

                    }
                    else
                    {
                        OnboardingActivity.CloseProgressDialog();
                        OnboardingActivity.ShowErrorDialog("Something went wrong, please retry");
                    }

                }));
            }
            else
            {
                return;
            }
        }

        private void PicDisplayFragment_RetakePic(object sender, EventArgs e)
        {
            CheckAndStartCamera();
            picDisplayFragment.DismissAllowingStateLoss();
        }

        private void UpdateUiOnCpture(CaptureType captureType)
        {
            switch (captureType)
            {
                case CaptureType.ProfilePic:
                    Card1.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                    NxtImg1.ImageTintList = ColorStateList.ValueOf(Color.Blue);
                    break;
                case CaptureType.FrontOfLicense:
                    Card2.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                    NxtImg2.ImageTintList = ColorStateList.ValueOf(Color.Blue);
                    break;
                case CaptureType.BackOfLicense:
                    Card3.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                    NxtImg3.ImageTintList = ColorStateList.ValueOf(Color.Blue);
                    break;
            }

            OnboardingActivity.CloseProgressDialog();
        }

        private void UpdateUiOnError(CaptureType captureType)
        {
            switch (captureType)
            {
                case CaptureType.ProfilePic:
                    Card1.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                    NxtImg1.SetImageResource(Resource.Drawable.ic_error);
                    HeaderTxt1.SetText(Resource.String.string_attention_seekr);
                    break;
                case CaptureType.FrontOfLicense:
                    Card2.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                    NxtImg2.SetImageResource(Resource.Drawable.ic_error);
                    HeaderTxt2.SetText(Resource.String.string_attention_seekr);
                    break;
                case CaptureType.BackOfLicense:
                    Card3.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                    NxtImg3.SetImageResource(Resource.Drawable.ic_error);
                    HeaderTxt3.SetText(Resource.String.string_attention_seekr);
                    break;
            }
            OnboardingActivity.CloseProgressDialog();
        }

        internal sealed class ContinuationTask : Java.Lang.Object, IContinuation
        {
            private Action<Task> _then;

            public ContinuationTask(Action<Task> then)
            {
                _then = then;
            }

            public Java.Lang.Object Then(Task task)
            {
                _then?.Invoke(task);
                return imageRef.GetDownloadUrl();
            }
        }
    }
}