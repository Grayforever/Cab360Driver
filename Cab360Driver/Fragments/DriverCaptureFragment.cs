using Android;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Cab360Driver.Helpers;
using Firebase.Storage;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cab360Driver.Fragments
{
    public class DriverCaptureFragment : AndroidX.Fragment.App.Fragment, Android.Gms.Tasks.IOnSuccessListener, Android.Gms.Tasks.IOnFailureListener
    {
        private CardView Card1, Card2, Card3;
        private ImageView NxtImg1, NxtImg2, NxtImg3;
        private Bitmap bitmapProfile;
        private TextView HeaderTxt1, HeaderTxt2, HeaderTxt3, MessageTxt1, MessageTxt2, MessageTxt3;

        private int whichIsCaptured;
        bool _isCamLoaded = false;

        public event EventHandler ProfileCaptured;

        

        public const int RequestCode = 100;
        public const int RequestPermission = 200;

        private BeforeUSnapFragment CameraIntroDialog = new BeforeUSnapFragment();
        private BeforeUSnap2Fragment LicenseIntroDialog = new BeforeUSnap2Fragment();
        private PicDisplayFragment picDisplayFragment;
        private StorageReference StoreRef;
        private FirebaseStorage FireStorage;
        private byte[] imageArray;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.driver_capture_layout, container, false);
            GetControls(view);
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(new string[]
                {
                    Manifest.Permission.Camera
                }, RequestPermission);
            }
            FireStorage = FirebaseStorage.Instance;
            StoreRef = FireStorage.GetReferenceFromUrl("gs://taxiproject-185a4.appspot.com");

            
        }

        private void GetControls(View view)
        {
            Card1 = view.FindViewById<CardView>(Resource.Id.req_c1);
            Card1.Click += Card1_Click;

            Card2 = view.FindViewById<CardView>(Resource.Id.req_c2);
            Card2.Click += Card2_Click;
            Card3 = view.FindViewById<CardView>(Resource.Id.req_c3);

            NxtImg1 = view.FindViewById<ImageView>(Resource.Id.nxt_img1);
            NxtImg2 = view.FindViewById<ImageView>(Resource.Id.nxt_img2);
            NxtImg3 = view.FindViewById<ImageView>(Resource.Id.nxt_img3);

            var WelcomeTxt = view.FindViewById<TextView>(Resource.Id.drv_capt_welc_tv);

            HeaderTxt1 = view.FindViewById<TextView>(Resource.Id.rec_text);
            HeaderTxt2 = view.FindViewById<TextView>(Resource.Id.rec_text2);
            HeaderTxt3 = view.FindViewById<TextView>(Resource.Id.rec_text3);

            MessageTxt1 = view.FindViewById<TextView>(Resource.Id.drv_photo_txt);
            MessageTxt2 = view.FindViewById<TextView>(Resource.Id.drv_photo_txt);
            MessageTxt3 = view.FindViewById<TextView>(Resource.Id.drv_photo_txt);
        }

        private void Card2_Click(object sender, EventArgs e)
        {
            
            whichIsCaptured = 2;
            //BeginLicenseCapture();
        }

        private void Card1_Click(object sender, EventArgs e)
        {
            whichIsCaptured = 1;
            BeginProfileCapture();

        }

        private void BeginLicenseCapture()
        {
            //
            //LicenseIntroDialog.Show(FragmentManager, "showLicenceCapture");
            //LicenseIntroDialog.StartLicenseCamera += LicenseIntroDialog_StartLicenseCamera;
        }

        private void LicenseIntroDialog_StartLicenseCamera(object sender, EventArgs e)
        {
            CheckAndStartCamera();
        }

        private void BeginProfileCapture()
        {
            
            CameraIntroDialog.Show(Activity.SupportFragmentManager, "showCameraAction");
            CameraIntroDialog.StartCameraAsync += CameraIntroDialog_StartCameraAsync;
        }

        private void CameraIntroDialog_StartCameraAsync(object sender, EventArgs e)
        {
            CheckAndStartCamera();
        }

        private async void CheckAndStartCamera()
        {
            if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
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
                catch(Exception e)
                {
                    Toast.MakeText(Activity, e.Message, ToastLength.Short).Show();
                }
                cts = null;
            }
            else
            {

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

            if (file == null)
            {
                return 0;
            }
                
            imageArray = await System.IO.File.ReadAllBytesAsync(file.Path, ct);

            bitmapProfile = await BitmapFactory.DecodeByteArrayAsync(imageArray, 0, imageArray.Length);

            picDisplayFragment = new PicDisplayFragment(bitmapProfile);
            picDisplayFragment.Cancelable = false;
            var trans = Activity.SupportFragmentManager.BeginTransaction();
            picDisplayFragment.Show(trans, "Pic_display_fragment");
            picDisplayFragment.SavePic += PicDisplayFragment_SavePic;
            picDisplayFragment.RetakePic += PicDisplayFragment_RetakePic;

            return bitmapProfile.ByteCount;
        }

        private void PicDisplayFragment_SavePic(object sender, PicDisplayFragment.HasImageEventArgs e)
        {
            if(e.viewHasImage == true)
            {

                var image = StoreRef.Child("driverProfilePics/" + AppDataHelper.GetCurrentUser().Uid);
                UploadTask uploadTask = image.PutBytes(imageArray);
                uploadTask.AddOnSuccessListener(this);
                uploadTask.AddOnFailureListener(this);
            }
            else
            {
                Toast.MakeText(Application.Context, "no image to save", ToastLength.Long).Show();
            }
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            UpdateUiOnCpture(1);

            ProfileCaptured.Invoke(this, new EventArgs());
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            UpdateUiOnError(1);
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