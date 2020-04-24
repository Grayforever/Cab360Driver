using Android;
using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Cab360Driver.DataModels;
using Cab360Driver.EventListeners;
using Cab360Driver.Fragments;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Google.Android.Material.Button;
using Google.Android.Material.Snackbar;
using Java.Util;
using System;

namespace Cab360Driver.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false, Theme ="@style/AppTheme", ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize| SoftInput.StateHidden)]
    public class OnboardingActivity : AppCompatActivity, IOnSuccessListener, IOnFailureListener
    {
        private MaterialButton SignInBtn;
        private MaterialButton RegisterBtn;

        
        private TaskCompletionListeners TaskCompletionListener = new TaskCompletionListeners();
        private LinearLayout driverOnboardingRoot;

        private FirebaseDatabase FireDatabase;
        private FirebaseAuth FireAuth;
        
        private DatabaseReference driverRef;

        private string stage;
        private bool shouldCauseExit = false;

        private readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.container_main);
            GetControls();

            FireDatabase = AppDataHelper.GetDatabase();
            
            FireAuth = AppDataHelper.GetFirebaseAuth();
            stage = AppDataHelper.GetStage();

            RequestPermissions(permissionGroup, 0);

            GetStage(stage);
        }

        private void GetStage(string stage)
        {
            if (!string.IsNullOrEmpty(stage))
            {
                if(stage == "1")
                {
                    DriverPartnerFragment PartnerFragment = new DriverPartnerFragment();
                    shouldCauseExit = true;
                    ShowFragment(PartnerFragment);
                    PartnerFragment.StageTwoPassEvent += PartnerFragment_StageTwoPassEvent;
                }
                else if(stage == "2")
                {
                    shouldCauseExit = true;
                    DriverCaptureFragment DriverCaptureFragment = new DriverCaptureFragment();
                    ShowFragment(DriverCaptureFragment);
                    DriverCaptureFragment.OnProfileCaptured += DriverCaptureFragment_OnProfileCaptured;
                }
                else if (stage == "3")
                {
                    shouldCauseExit = true;
                    CarRegFragment carRegFragment = new CarRegFragment();
                    ShowFragment(carRegFragment);
                    carRegFragment.OnCardetailsSaved += CarRegFragment_OnCardetailsSaved;
                }
            }
            else
            {
                return;
            }
        }

        private void GetControls()
        {
            driverOnboardingRoot = (LinearLayout)FindViewById(Resource.Id.driver_onboarding_root);
            SignInBtn = FindViewById<MaterialButton>(Resource.Id.onbd_signin_btn);
            SignInBtn.Click += SignInBtn_Click;
            RegisterBtn = FindViewById<MaterialButton>(Resource.Id.onbd_signup_btn);
            RegisterBtn.Click += RegisterBtn_Click;
        }

        private void SignInBtn_Click(object sender, EventArgs e)
        {
            DriverSignInFragment SignInFragment = new DriverSignInFragment();
            ShowFragment(SignInFragment);
        }

        private void ShowFragment(AndroidX.Fragment.App.Fragment fragment)
        {
            if (fragment == null)
                return;

            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment)
                .AddToBackStack(null)
                .Commit();
        }

        private void RegisterBtn_Click(object sender, EventArgs e)
        {
            DriverRegisterFragment RegisterFragment = new DriverRegisterFragment();
            ShowFragment(RegisterFragment);
            RegisterFragment.StageOnePassEvent += RegisterFragment_StageOnePassEvent;
        }

        private void RegisterFragment_StageOnePassEvent(object sender, DriverRegisterFragment.StageOnePassEventArgs e)
        {
            var driver = e.driver;
            if(driver != null)
            {
                SaveDriverPersonal(driver);
            }
            else
            {
                Snackbar.Make(driverOnboardingRoot, "Error", Snackbar.LengthShort).Show();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void SaveDriverPersonal(Driver driverPersonal)
        {
            HashMap Cab360Drivers = new HashMap();
            Cab360Drivers.Put("firstname", driverPersonal.Fname);
            Cab360Drivers.Put("lastname", driverPersonal.Lname);
            Cab360Drivers.Put("email", driverPersonal.Email);
            Cab360Drivers.Put("phone", driverPersonal.Phone);
            Cab360Drivers.Put("city", driverPersonal.City);
            Cab360Drivers.Put("invitecode", driverPersonal.Code);
            Cab360Drivers.Put("created_at", DateTime.UtcNow.ToString());
            Cab360Drivers.Put("isPartner", driverPersonal.IsPartner);
            Cab360Drivers.Put("stage_of_registration", "0");
            SaveDriverToDb(Cab360Drivers);
        }

        private void SaveDriverToDb(HashMap cab360Drivers)
        {
            driverRef = FireDatabase.GetReference("Cab360Drivers/" + FireAuth.CurrentUser.Uid);
            driverRef.SetValue(cab360Drivers)
                .AddOnSuccessListener(this)
                .AddOnFailureListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            if (driverRef == null)
                driverRef = FireDatabase.GetReference("Cab360Drivers/" + FireAuth.CurrentUser.Uid);

            driverRef.Child("stage_of_registration").SetValue("1")
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);

            TaskCompletionListener.Successful += TaskCompletionListener_Successful3;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure3;
        }

        private void TaskCompletionListener_Failure3(object sender, EventArgs e)
        {
            Snackbar.Make(driverOnboardingRoot, "Error", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Successful3(object sender, TaskCompletionListeners.ResultArgs e)
        {
            DriverPartnerFragment PartnerFragment = new DriverPartnerFragment();
            ShowFragment(PartnerFragment);
            PartnerFragment.StageTwoPassEvent += PartnerFragment_StageTwoPassEvent;
        }

        private void PartnerFragment_StageTwoPassEvent(object sender, DriverPartnerFragment.StageTwoEventArgs e)
        {
            var val = e.IsPartner;
            if (val == 0)
            {
                Snackbar.Make(driverOnboardingRoot, "Error", Snackbar.LengthShort).Show();
            }
            else if (val == 1)
            {
                if (driverRef == null)
                    driverRef = FireDatabase.GetReference("Cab360Drivers/" + FireAuth.CurrentUser.Uid);

                driverRef.Child("isPartner").SetValue(val.ToString())
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
                TaskCompletionListener.Successful += TaskCompletionListener_Successful;
                TaskCompletionListener.Failure += TaskCompletionListener_Failure;

            }
            else if (val == 2)
            {
                if (driverRef == null)
                    driverRef = FireDatabase.GetReference("Cab360Drivers/" + FireAuth.CurrentUser.Uid);

                driverRef.Child("isPartner").SetValue(val.ToString())
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);

                TaskCompletionListener.Successful += TaskCompletionListener_Successful;
                TaskCompletionListener.Failure += TaskCompletionListener_Failure;
            }
            
        }

        //stage partnership s2
        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(driverOnboardingRoot, "Error", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            if (driverRef == null)
                driverRef = FireDatabase.GetReference("Cab360Drivers/" + FireAuth.CurrentUser.Uid);

            driverRef.Child("stage_of_registration").SetValue("2")
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful1;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure1;
        }

        //pass to s3
        private void TaskCompletionListener_Failure1(object sender, EventArgs e)
        {
            Snackbar.Make(driverOnboardingRoot, "Error", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Successful1(object sender, TaskCompletionListeners.ResultArgs e)
        {
            //DriverCaptureFragment = null;
            DriverCaptureFragment DriverCaptureFragment = new DriverCaptureFragment();
            ShowFragment(DriverCaptureFragment);
            DriverCaptureFragment.OnProfileCaptured += DriverCaptureFragment_OnProfileCaptured;

        }

        private void DriverCaptureFragment_OnProfileCaptured(object sender, EventArgs e)
        {
            if (driverRef == null)
                driverRef = FireDatabase.GetReference("Cab360Drivers/" + FireAuth.CurrentUser.Uid); 

            driverRef.Child("stage_of_registration").SetValue("3")
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful2;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure2;
        }

        private void TaskCompletionListener_Failure2(object sender, EventArgs e)
        {
            Snackbar.Make(driverOnboardingRoot, "Error", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Successful2(object sender, TaskCompletionListeners.ResultArgs e)
        {
            CarRegFragment carRegFragment = new CarRegFragment();
            ShowFragment(carRegFragment);
            carRegFragment.OnCardetailsSaved += CarRegFragment_OnCardetailsSaved;
        }

        private void CarRegFragment_OnCardetailsSaved(object sender, CarRegFragment.CarModelArgs e)
        {
            HashMap carMap = new HashMap();
            carMap.Put("car_model", e.CarDetails.Model);
            carMap.Put("car_brand", e.CarDetails.Brand);
            carMap.Put("car_year", e.CarDetails.Year);
            carMap.Put("car_color", e.CarDetails.Color);
            carMap.Put("car_condition", e.CarDetails.Condition);
            carMap.Put("curr_user", e.CarDetails.CurrUser);
            carMap.Put("reg_no", e.CarDetails.RegNo);

            SaveCarDetailsToDb(carMap);
        }

        private void SaveCarDetailsToDb(HashMap carMap)
        {
            driverRef = FireDatabase.GetReference("RegUnVerifiedCars/" + FireAuth.CurrentUser.Uid);
            driverRef.SetValue(carMap)
                .AddOnSuccessListener(TaskCompletionListener)
                .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful4;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure4;
        }

        private void TaskCompletionListener_Failure4(object sender, EventArgs e)
        {
            SayToast("We couldnt save your cars");
        }

        private void SayToast(string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }

        private void TaskCompletionListener_Successful4(object sender, TaskCompletionListeners.ResultArgs e)
        {
            if (driverRef == null)
                driverRef = FireDatabase.GetReference("Cab360Drivers/" + FireAuth.CurrentUser.Uid);

            driverRef.Child("stage_of_registration").SetValue("4")
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful5;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure5;
        }

        private void TaskCompletionListener_Failure5(object sender, EventArgs e)
        {
            SayToast("We couldnt change the stage from 3 to 4");
        }

        private void TaskCompletionListener_Successful5(object sender, TaskCompletionListeners.ResultArgs e)
        {
            CarPicsFragment carPicsFragment = new CarPicsFragment();
            ShowFragment(carPicsFragment);
        }

        public override void OnBackPressed()
        {
            
            base.OnBackPressed();
            if(shouldCauseExit == true)
            {
                Finish();
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            
        }

        

    }
}