using Android;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using CancellationToken = System.Threading.CancellationToken;
using CancellationTokenSource = System.Threading.CancellationTokenSource;

namespace Cab360Driver.Fragments
{
    public class DriverCaptureFragment : AndroidX.Fragment.App.Fragment
    {
        private CardView Card1, Card2, Card3;
        private ImageView NxtImg1, NxtImg2, NxtImg3;
        private TextView HeaderTxt1; 
        private TextView HeaderTxt2;
        private TextView HeaderTxt3;
        FirebaseAuth FireAuth;
        DatabaseReference driverRef;
        private TaskCompletionListeners TaskCompletionListener = new TaskCompletionListeners();

        public event EventHandler ProfileCaptured;

        public const int RequestCode = 100;
        public const int RequestPermission = 200;

        private BeforeUSnapFragment CameraIntroDialog = new BeforeUSnapFragment();
        private PicDisplayFragment picDisplayFragment;
        private StorageReference StoreRef;
        private FirebaseStorage FireStorage;
        private byte[] imageArray;

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
            FireAuth = AppDataHelper.GetFirebaseAuth();
            driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid);
            FireStorage = FirebaseStorage.Instance;
            StoreRef = FireStorage.GetReferenceFromUrl("gs://taxiproject-185a4.appspot.com");
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
            welcomeTV.Text = $"Welcome, {AppDataHelper.GetFirstName()}.";
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
            if(captureType == CaptureType.ProfilePic)
            {
                CameraIntroDialog.Show(Activity.SupportFragmentManager, "ProfileCapture");
                CameraIntroDialog.StartCameraAsync += CameraIntroDialog_StartCameraAsync;
            }
            else if(captureType == CaptureType.FrontOfLicense)
            {

            }
            else if(captureType == CaptureType.BackOfLicense)
            {

            }
        }

        private void CameraIntroDialog_StartCameraAsync(object sender, BeforeUSnapFragment.CamArgs e)
        {
            CheckAndStartCamera();
        }

        private async void CheckAndStartCamera()
        {
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                try
                {
                    int bytecount = await TakePhotoAsync(cts.Token);
                    if (bytecount != 0)
                        cts.Cancel();
                }
                catch (System.OperationCanceledException oce)
                {
                    Toast.MakeText(Activity, oce.Message, ToastLength.Short).Show();
                }
                catch(System.Exception e)
                {
                    Toast.MakeText(Activity, e.Message, ToastLength.Short).Show();
                }
                cts = null;
            }
            else
            {
                ShouldShowRequestPermissionRationale(Manifest.Permission.Camera);
            }
        }

        private async Task<int> TakePhotoAsync(CancellationToken ct)
        {
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                CompressionQuality = 60,
                Name = "myimage.jpg",
                Directory = "sample",
            }, ct);

            if (file != null)
            {
                imageArray = await File.ReadAllBytesAsync(file.Path, ct);
                var bitmapProfile = await BitmapFactory.DecodeByteArrayAsync(imageArray, 0, imageArray.Length);
                DisplayPic(bitmapProfile);

                return bitmapProfile.ByteCount;
            }
            return 0;
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
                var image = StoreRef.Child("driverProfilePics/" + AppDataHelper.GetCurrentUser().Uid);
                UploadTask uploadTask = image.PutBytes(imageArray);
                uploadTask.AddOnSuccessListener(TaskCompletionListener);
                TaskCompletionListener.Successful += TaskCompletionListener_Successful;
                uploadTask.AddOnFailureListener(TaskCompletionListener);
                TaskCompletionListener.Failure += TaskCompletionListener_Failure;
            }
            else
            {
                Toast.MakeText(Activity, "no image to save", ToastLength.Short).Show();
            }
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            UpdateUiOnError(CaptureType.ProfilePic);
        }

        private void TaskCompletionListener_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            driverRef.Child("stage_of_registration").SetValue(RegistrationStage.CarRegistering.ToString())
               .AddOnSuccessListener(TaskCompletionListener)
               .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful1;
        }


        private void TaskCompletionListener_Successful1(object sender, TaskCompletionListeners.ResultArgs e)
        {
            ProfileCaptured.Invoke(this, new EventArgs());
            UpdateUiOnCpture(CaptureType.ProfilePic);
        }


        private void PicDisplayFragment_RetakePic(object sender, EventArgs e)
        {
            CheckAndStartCamera();
        }

        private void UpdateUiOnCpture(CaptureType captureType)
        {
            if (captureType == CaptureType.ProfilePic)
            {
                Card1.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                NxtImg1.ImageTintList = ColorStateList.ValueOf(Color.Blue);
            }
            else if (captureType == CaptureType.FrontOfLicense)
            {
                Card2.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                NxtImg2.ImageTintList = ColorStateList.ValueOf(Color.Blue);
            }
            else if (captureType == CaptureType.BackOfLicense)
            {
                Card3.CardBackgroundColor = ColorStateList.ValueOf(Color.LightSkyBlue);
                NxtImg3.ImageTintList = ColorStateList.ValueOf(Color.Blue);
            }
        }

        private void UpdateUiOnError(CaptureType captureType)
        {
            if (captureType == CaptureType.ProfilePic)
            {
                Card1.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                NxtImg1.SetImageResource(Resource.Drawable.ic_error);
                HeaderTxt1.SetText(Resource.String.string_attention_seekr);
            }
            else if (captureType == CaptureType.FrontOfLicense)
            {
                Card2.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                NxtImg2.SetImageResource(Resource.Drawable.ic_error);
                HeaderTxt2.SetText(Resource.String.string_attention_seekr);
            }
            else if (captureType == CaptureType.BackOfLicense)
            {
                Card3.CardBackgroundColor = ColorStateList.ValueOf(Color.PapayaWhip);
                NxtImg3.SetImageResource(Resource.Drawable.ic_error);
                HeaderTxt3.SetText(Resource.String.string_attention_seekr);
            }
        }
    }
}