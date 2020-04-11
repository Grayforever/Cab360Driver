using Android;
using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Cab360Driver.DataModels;
using Cab360Driver.EventListeners;
using Cab360Driver.Fragments;
using Cab360Driver.Helpers;
using CN.Pedant.SweetAlert;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Java.Util;
using System;

namespace Cab360Driver.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false, Theme ="@style/AppTheme", ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class OnboardingActivity : AppCompatActivity, IOnSuccessListener, IOnFailureListener
    {
        private Button SignInBtn;
        private Button RegisterBtn;
        private DriverRegisterFragment RegisterFragment = new DriverRegisterFragment();
        private DriverSignInFragment SignInFragment = new DriverSignInFragment();
        private DriverPartnerFragment PartnerFragment = new DriverPartnerFragment();
        private DriverCaptureFragment DriverCaptureFragment = new DriverCaptureFragment();
        private TaskCompletionListeners TaskCompletionListener = new TaskCompletionListeners();

        private FirebaseDatabase FireDatabase;
        private FirebaseAuth FireAuth;
        
        private DatabaseReference driverRef;

        private SweetAlertDialog progressDialog;
        private SweetAlertDialog successDialog;

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

            progressDialog = new SweetAlertDialog(this, SweetAlertDialog.ProgressType);
            successDialog = new SweetAlertDialog(this, SweetAlertDialog.SuccessType);
            RequestPermissions(permissionGroup, 0);

            ShowProgress();
            GetStage(stage);

            
        }

        private void GetStage(string stage)
        {
            if (!string.IsNullOrEmpty(stage))
            {
                if(stage == "1")
                {
                    shouldCauseExit = true;
                    CloseProgress();
                    ShowFragment(PartnerFragment);
                    PartnerFragment.StageTwoPassEvent += PartnerFragment_StageTwoPassEvent;
                }
                else if(stage == "2")
                {
                    shouldCauseExit = true;
                    CloseProgress();
                    ShowFragment(DriverCaptureFragment);
                    DriverCaptureFragment.StageThreePassEvent += DriverCaptureFragment_StageThreePassEvent;
                }
            }
            else
            {
                CloseProgress();
                return;
            }
        }
 

        private void GetControls()
        {
            SignInBtn = FindViewById<Button>(Resource.Id.onbd_signin_btn);
            SignInBtn.Click += SignInBtn_Click;
            RegisterBtn = FindViewById<Button>(Resource.Id.onbd_signup_btn);
            RegisterBtn.Click += RegisterBtn_Click;
        }

        private void SignInBtn_Click(object sender, EventArgs e)
        {
            
            ShowFragment(SignInFragment);
        }

        private void ShowFragment(Android.Support.V4.App.Fragment fragment)
        {
            SupportFragmentManager
                .BeginTransaction()
                .SetCustomAnimations(Resource.Animation.enter, Resource.Animation.exit, Resource.Animation.pop_enter, Resource.Animation.pop_exit)
                .Replace(Resource.Id.content_frame, fragment)
                .AddToBackStack(null)
                .Commit();
        }

        private void RegisterBtn_Click(object sender, EventArgs e)
        {
            ShowFragment(RegisterFragment);
            RegisterFragment.StageOnePassEvent += RegisterFragment_StageOnePassEvent;
        }

        private void RegisterFragment_StageOnePassEvent(object sender, DriverRegisterFragment.StageOnePassEventArgs e)
        {
            ShowProgress();
            var driver = e.driver;
            if(driver != null)
            {
                
                SaveDriverPersonal(driver);
            }
            else
            {
                ShowError("User account creation not successful. Please try again!");
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
            CloseProgress();
            driverRef.Child("stage_of_registration").SetValue("1")
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);

            TaskCompletionListener.Successful += TaskCompletionListener_Successful3;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure3;
        }

        private void TaskCompletionListener_Failure3(object sender, EventArgs e)
        {
            
        }

        private void TaskCompletionListener_Successful3(object sender, TaskCompletionListeners.ResultArgs e)
        {
            ShowFragment(PartnerFragment);
            PartnerFragment.StageTwoPassEvent += PartnerFragment_StageTwoPassEvent;
        }

        private void PartnerFragment_StageTwoPassEvent(object sender, DriverPartnerFragment.StageTwoEventArgs e)
        {
            var val = e.IsPartner;
            if (val == 0)
            {
                ShowError("No option selected");
            }
            else if (val == 1)
            {
                if (driverRef == null)
                    return;

                driverRef.Child("isPartner").SetValue(val.ToString())
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
                TaskCompletionListener.Successful += TaskCompletionListener_Successful;
                TaskCompletionListener.Failure += TaskCompletionListener_Failure;

            }
            else if (val == 2)
            {
                if (driverRef == null)
                    return;

                driverRef.Child("isPartner").SetValue(val.ToString())
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);

                TaskCompletionListener.Successful += TaskCompletionListener_Successful;
                TaskCompletionListener.Failure += TaskCompletionListener_Failure;
            }
            
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            ShowError("Something is preventing us to process your request");
        }

        private void TaskCompletionListener_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            driverRef.Child("stage_of_registration").SetValue("2")
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful1;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure1;
        }

        private void TaskCompletionListener_Failure1(object sender, EventArgs e)
        {
            ShowError("Something is preventing us to process your request");
        }

        private void TaskCompletionListener_Successful1(object sender, TaskCompletionListeners.ResultArgs e)
        {
            ShowFragment(DriverCaptureFragment);
            DriverCaptureFragment.StageThreePassEvent += DriverCaptureFragment_StageThreePassEvent;
        }

        private void DriverCaptureFragment_StageThreePassEvent(object sender, DriverCaptureFragment.StageThreeEventArgs e)
        {
            bool isAllCaptured = e._isAllCaptured;
            if(isAllCaptured == true)
            {
                driverRef.Child("stage_of_registration").SetValue("3")
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
                TaskCompletionListener.Successful += TaskCompletionListener_Successful2;
                TaskCompletionListener.Failure += TaskCompletionListener_Failure2;
            }
        }

        private void TaskCompletionListener_Failure2(object sender, EventArgs e)
        {
            ShowError("Something is preventing us to process your request");
        }

        private void TaskCompletionListener_Successful2(object sender, TaskCompletionListeners.ResultArgs e)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(intent);
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
            ShowError(e.Message.ToString());
        }

        private void ShowError(string message)
        {
            new SweetAlertDialog(this, SweetAlertDialog.ErrorType)
                .SetTitleText("Error")
                .SetContentText(message)
                .SetConfirmText("OK")
                .SetConfirmClickListener(null)
                .Show();
        }

        private void ShowProgress()
        {
            progressDialog.ProgressHelper.BarColor = Resource.Color.colorAccent;
            progressDialog.SetTitleText("Loading");
            progressDialog.SetCancelable(false);
            progressDialog.Show();
        }

        private void CloseProgress()
        {
            if(progressDialog.IsShowing == true)
            {
                progressDialog.DismissWithAnimation();
            }
        }

        private void ShowSuccess(string title)
        {
            successDialog.SetTitleText(title);
            successDialog.SetConfirmText("OK");
            successDialog.SetConfirmClickListener(null);
            successDialog.SetCancelable(false);
            successDialog.Show();

        }
    }
}