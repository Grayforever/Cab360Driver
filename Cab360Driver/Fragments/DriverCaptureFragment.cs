using Android;
using Android.App;
using Android.Content.Res;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using Cab360Driver.Helpers;
using Firebase.ML.Vision;
using Firebase.ML.Vision.Common;
using Firebase.ML.Vision.Face;
using Firebase.Storage;
using Plugin.Media;
using System;
using static Firebase.Storage.UploadTask;

namespace Cab360Driver.Fragments
{
    public class DriverCaptureFragment : Android.Support.V4.App.Fragment, IOnSuccessListener, IOnFailureListener
    {
        private CardView Card1, Card2, Card3;
        private ImageView NxtImg1, NxtImg2, NxtImg3;
        private Bitmap bitmapProfile;
        private TextView HeaderTxt1, HeaderTxt2, HeaderTxt3, MessageTxt1, MessageTxt2, MessageTxt3;
        private ProgressBar progressBar;

        private int whichIsCaptured = 0;

        private bool IsAllCaptured = false;

        public const int RequestCode = 100;
        public const int RequestPermission = 200;

        private BeforeUSnapFragment CameraIntroDialog = new BeforeUSnapFragment();
        private BeforeUSnap2Fragment LicenseIntroDialog = new BeforeUSnap2Fragment();
        private PicDisplayFragment picDisplayFragment;
        private StorageReference StoreRef;
        private FirebaseStorage FireStorage;
        private byte[] imageArray;

        public class StageThreeEventArgs : EventArgs
        {
            public bool _isAllCaptured { get; set; }
 
        }

        public event EventHandler<StageThreeEventArgs> StageThreePassEvent;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                //request permission
                RequestPermissions(new string[]
                {
                    Manifest.Permission.Camera
                }, RequestPermission);

                
            }
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            FireStorage = FirebaseStorage.Instance;
            StoreRef = FireStorage.GetReferenceFromUrl("gs://taxiproject-185a4.appspot.com");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.driver_capture_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            GetControls(view);
        }

        private void GetControls(View view)
        {
            Card1 = view.FindViewById<CardView>(Resource.Id.req_c1);
            Card1.Click += Card1_Click;

            Card2 = view.FindViewById<CardView>(Resource.Id.req_c2);
            Card2.Click += Card2_Click;
            Card3 = view.FindViewById<CardView>(Resource.Id.req_c3);
            Card3.Click += Card3_Click;

            NxtImg1 = view.FindViewById<ImageView>(Resource.Id.nxt_img1);
            NxtImg2 = view.FindViewById<ImageView>(Resource.Id.nxt_img2);
            NxtImg3 = view.FindViewById<ImageView>(Resource.Id.nxt_img3);

            var WelcomeTxt = view.FindViewById<TextView>(Resource.Id.drv_capt_welc_tv);
            WelcomeTxt.Text = $"Welcome, {AppDataHelper.GetFirebaseAuth().CurrentUser.DisplayName}";

            HeaderTxt1 = view.FindViewById<TextView>(Resource.Id.rec_text);
            HeaderTxt2 = view.FindViewById<TextView>(Resource.Id.rec_text2);
            HeaderTxt3 = view.FindViewById<TextView>(Resource.Id.rec_text3);

            MessageTxt1 = view.FindViewById<TextView>(Resource.Id.drv_photo_txt);
            MessageTxt2 = view.FindViewById<TextView>(Resource.Id.drv_photo_txt);
            MessageTxt3 = view.FindViewById<TextView>(Resource.Id.drv_photo_txt);

            progressBar = view.FindViewById<ProgressBar>(Resource.Id.prog1);
        }

        private void Card3_Click(object sender, EventArgs e)
        {
            whichIsCaptured = 3;
            BeginLicenseCapture();
        }

        private void Card2_Click(object sender, EventArgs e)
        {
            
            whichIsCaptured = 2;
            BeginLicenseCapture();
        }

        private void Card1_Click(object sender, EventArgs e)
        {
            whichIsCaptured = 1;
            BeginProfileCapture();

        }

        private void BeginLicenseCapture()
        {
            progressBar.Visibility = ViewStates.Visible;
            LicenseIntroDialog.Show(FragmentManager, "showLicenceCapture");
            LicenseIntroDialog.StartLicenseCamera += LicenseIntroDialog_StartLicenseCamera;
        }

        private void LicenseIntroDialog_StartLicenseCamera(object sender, EventArgs e)
        {
            CheckAndStartCamera();
        }

        private void BeginProfileCapture()
        {
            progressBar.Visibility = ViewStates.Visible;
            CameraIntroDialog.Show(FragmentManager, "showCameraAction");
            CameraIntroDialog.StartCameraAsync += CameraIntroDialog_StartCameraAsync;
        }

        private void CameraIntroDialog_StartCameraAsync(object sender, EventArgs e)
        {
            CheckAndStartCamera();
        }

        private void CheckAndStartCamera()
        {
            try
            {
                if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
                {
                    TakePhoto();
                }
                else
                {

                }
            }
            catch (InvalidOperationException)
            {

            }
        }

        private async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 60,
                Name = "myimage.jpg",
                Directory = "sample",
            });

            if (file == null)
            {
                progressBar.Visibility = ViewStates.Invisible;
                return;
            }
                
            imageArray = System.IO.File.ReadAllBytes(file.Path);

            bitmapProfile = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);


            picDisplayFragment = new PicDisplayFragment(bitmapProfile);
            picDisplayFragment.Cancelable = false;
            var trans = FragmentManager.BeginTransaction();
            picDisplayFragment.Show(trans, "Pic_display_fragment");
            picDisplayFragment.SavePic += PicDisplayFragment_SavePic;
            picDisplayFragment.RetakePic += PicDisplayFragment_RetakePic;
            
            
        }

        private void PicDisplayFragment_SavePic(object sender, PicDisplayFragment.HasImageEventArgs e)
        {
            if(e.viewHasImage == true)
            {
                var image = StoreRef.Child("driverProfilePics/" + AppDataHelper.GetCurrentUser().Uid);
                UploadTask uploadTask = image.PutBytes(imageArray);
                uploadTask.AddOnSuccessListener(this);
                uploadTask.AddOnFailureListener(this);

                RunDetector(bitmapProfile);
                
            }
            else
            {
                Toast.MakeText(Application.Context, "no image to save", ToastLength.Long).Show();
            }
        }

        private void RunDetector(Bitmap bitmap)
        {
            var image = FirebaseVisionImage.FromBitmap(bitmap);

            var options = new FirebaseVisionFaceDetectorOptions.Builder()
                .Build();

            var detector = FirebaseVision.Instance.GetVisionFaceDetector(options);

            detector.DetectInImage(image)
                .AddOnSuccessListener(this)
                .AddOnFailureListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            var face = result.JavaCast<TaskSnapshot>();

        }

        public void OnFailure(Java.Lang.Exception e)
        {
            
        }

        private void PicDisplayFragment_RetakePic(object sender, EventArgs e)
        {
            CheckAndStartCamera();
        }

        private void UpdateUiOnCpture(int whoToUpdate)
        {
            switch (bitmapProfile)
            {
                case null:
                    return;
                default:
                    switch (whoToUpdate)
                    {
                        case 1:
                            Card1.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                            NxtImg1.ImageTintList = ColorStateList.ValueOf(Color.Blue);
                            break;

                        case 2:
                            Card2.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                            NxtImg2.ImageTintList = ColorStateList.ValueOf(Color.Blue);
                            break;

                        case 3:
                            Card3.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                            NxtImg3.ImageTintList = ColorStateList.ValueOf(Color.Blue);
                            break;

                        default:
                            break;
                    }

                    break;
            }

        }

        private void UpdateUiOnError(int whoToUpdate)
        {
            switch (bitmapProfile)
            {
                case null:
                    return;

                default:
                    switch (whoToUpdate)
                    {
                        case 1:
                            Card1.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                            NxtImg1.SetImageResource(Resource.Drawable.ic_error);
                            HeaderTxt1.Text = "Needs your attention";
                            break;

                        case 2:
                            Card2.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                            NxtImg2.SetImageResource(Resource.Drawable.ic_error);
                            HeaderTxt2.Text = "Needs your attention";

                            break;
                        case 3:
                            Card3.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                            NxtImg3.SetImageResource(Resource.Drawable.ic_error);
                            HeaderTxt3.Text = "Needs your attention";
                            break;

                    }
                    break;
            }
        } 
    }
}