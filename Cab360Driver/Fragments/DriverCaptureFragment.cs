using Android;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using Cab360Driver.Activities;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;

namespace Cab360Driver.Fragments
{
    public class DriverCaptureFragment : AndroidX.Fragment.App.Fragment
    {
        private CardView Card1, Card2, Card3;
        private ImageView NxtImg1, NxtImg2, NxtImg3;
        private TextView HeaderTxt1; 
        private TextView HeaderTxt2;
        private TextView HeaderTxt3;
        

        public event EventHandler ProfileCaptured;

        public const int RequestCode = 100;
        public const int RequestPermission = 200;

        private BeforeUSnapFragment CameraIntroDialog = new BeforeUSnapFragment();
        private PicDisplayFragment picDisplayFragment;
        private StorageReference StoreRef;
        private FirebaseStorage FireStorage;
        private FirebaseUser mUser;
        private DatabaseReference driverRef;
        private FirebaseDatabase fireDb;
        private byte[] imageArray;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mUser = AppDataHelper.GetCurrentUser();
            fireDb = AppDataHelper.GetDatabase();
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
            var view = inflater.Inflate(Resource.Layout.driver_capture_layout, container, false);
            GetControls(view);
            return view;
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
            GreetUser(WelcomeTxt);

            HeaderTxt1 = view.FindViewById<TextView>(Resource.Id.rec_text);
            HeaderTxt2 = view.FindViewById<TextView>(Resource.Id.rec_text2);
            HeaderTxt3 = view.FindViewById<TextView>(Resource.Id.rec_text3);
        }

        private void GreetUser(TextView welcomeTV)
        {
            string firstname = AppDataHelper.Firstname;
            welcomeTV.Text = $"Welcome, {firstname}.";
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
            //OnboardingActivity.ShowProgressDialog();
            switch (captureType)
            {
                case CaptureType.ProfilePic:
                    CameraIntroDialog.Show(Activity.SupportFragmentManager, "ProfileCapture");
                    CameraIntroDialog.StartCameraAsync += CameraIntroDialog_StartCameraAsync;
                    break;
                case CaptureType.FrontOfLicense:
                    break;
                case CaptureType.BackOfLicense:
                    break;
            }
        }

        private void CameraIntroDialog_StartCameraAsync(object sender, BeforeUSnapFragment.CamArgs e)
        {
            CheckAndStartCamera();
        }

        private void CheckAndStartCamera()
        {
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
            {
                try
                {
                    TakePhotoAsync();
                }
                catch(Exception e)
                {
                    Toast.MakeText(Activity, e.Message, ToastLength.Short).Show();
                }
            }
            else
            {
                ShouldShowRequestPermissionRationale(Manifest.Permission.Camera);
            }
        }

        private async void TakePhotoAsync()
        {
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                CompressionQuality = 60,
                Name = "myimage.jpg",
                Directory = "sample",
            });

            if (file != null)
            {
                imageArray = await File.ReadAllBytesAsync(file.Path);
                var bitmapProfile = await BitmapFactory.DecodeByteArrayAsync(imageArray, 0, imageArray.Length);
                DisplayPic(bitmapProfile);
            }
        }  

        private void DisplayPic(Bitmap bitmap)
        {
            picDisplayFragment = new PicDisplayFragment(bitmap);
            picDisplayFragment.Cancelable = false;
            picDisplayFragment.Show(Activity.SupportFragmentManager, "Pic_display_fragment");
            picDisplayFragment.SavePic += PicDisplayFragment_SavePic;
            picDisplayFragment.RetakePic += PicDisplayFragment_RetakePic;
        }

        private void PicDisplayFragment_SavePic(object sender, PicDisplayFragment.HasImageEventArgs e)
        {
            if(e.viewHasImage == true)
            {
                FireStorage = FirebaseStorage.Instance;
                StoreRef = FireStorage.GetReferenceFromUrl("gs://taxiproject-185a4.appspot.com");
                var currUser = AppDataHelper.GetCurrentUser();
                if(currUser != null)
                {
                    var imageRef = StoreRef.Child("driverProfilePics/" + currUser.Uid);
                    UploadTask uploadTask = imageRef.PutBytes(imageArray);
                    uploadTask.AddOnSuccessListener(new OnSuccessListener(r1 =>
                    {
                        driverRef = fireDb.GetReference("Drivers").Child(mUser.Uid);
                        driverRef.Child("stage_of_registration").SetValue(RegistrationStage.CarRegistering.ToString())
                            .AddOnSuccessListener(new OnSuccessListener(r2 =>
                            {
                                ProfileCaptured.Invoke(this, new EventArgs());
                                UpdateUiOnCpture(CaptureType.ProfilePic);
                            }))
                            .AddOnFailureListener(new OnFailureListener(e2 =>
                            {
                                UpdateUiOnError(CaptureType.ProfilePic);
                                Toast.MakeText(Activity, e2.Message, ToastLength.Short).Show();
                            }));
                    }));
                    uploadTask.AddOnFailureListener(new OnFailureListener(e1 =>
                    {
                        Toast.MakeText(Activity, e1.Message, ToastLength.Short).Show();
                    }));
                }
                else
                {
                    return;
                }
                
            }
            else
            {
                Toast.MakeText(Activity, "no image to save", ToastLength.Short).Show();
            }
        }

        private void PicDisplayFragment_RetakePic(object sender, EventArgs e)
        {
            CheckAndStartCamera();
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
        }
    }
}