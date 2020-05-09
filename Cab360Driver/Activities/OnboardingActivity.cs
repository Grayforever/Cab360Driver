using Android;
using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.DataModels;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Fragments;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Java.Util;
using System;


namespace Cab360Driver.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false, Theme ="@style/AppTheme", 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize, 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
        WindowSoftInputMode = SoftInput.AdjustResize| SoftInput.StateHidden)]

    public class OnboardingActivity : FragmentActivity
    {
        //task completion listeners
        private TaskCompletionListeners SignUpCompletListener = new TaskCompletionListeners();
        private TaskCompletionListeners PartnerStateSavedSuccess = new TaskCompletionListeners();
        private TaskCompletionListeners ProfileCapturedSuccess = new TaskCompletionListeners();
        private TaskCompletionListeners CarCapturedSuccess = new TaskCompletionListeners();
        private TaskCompletionListeners TaskCompletionListener = new TaskCompletionListeners();

        private RegistrationFragmentsAdapter regAdapter;
        private ViewPager2 RegViewPager;

        //online auth and db
        private FirebaseDatabase FireDatabase;
        private FirebaseAuth FireAuth;
        private DatabaseReference driverRef;

        //fragments init
        public OnboardingFragment OnboardingFragment = new OnboardingFragment();
        public DriverSignInFragment SignInFragment = new DriverSignInFragment();
        public DriverRegisterFragment RegisterFragment = new DriverRegisterFragment();
        public DriverCaptureFragment CaptureFragment = new DriverCaptureFragment();
        public DriverPartnerFragment PartnerFragment = new DriverPartnerFragment();
        public CarRegFragment CarRegFragment = new CarRegFragment();
        public CarPicsFragment CarPicsFragment = new CarPicsFragment();
        private string stage;
        private bool shouldCauseExit = false;
        bool SmoothScroll = false;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.container_main);

            RegViewPager = FindViewById<ViewPager2>(Resource.Id.reg_viewpager1);
            RegViewPager.Orientation = ViewPager2.OrientationHorizontal;
            RegViewPager.OffscreenPageLimit = 3;
            RegViewPager.UserInputEnabled = false;

            SetViewPagerAdapter();

            
            FireDatabase = AppDataHelper.GetDatabase();
            
            FireAuth = AppDataHelper.GetFirebaseAuth();
            stage = AppDataHelper.GetStage();

            RequestPermissions(StringConstants.GetLocationPermissiongroup(), 0);

            GetStage(stage);
        }

        private void GetStage(string stage)
        {
            if (!string.IsNullOrEmpty(stage))
            {
                if (stage == "1")
                {
                    shouldCauseExit = true;
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(3, SmoothScroll);
                    PartnerFragment.PartnerSelected += PartnerFragment_PartnerSelected;
                }
                else if (stage == "2")
                {
                    shouldCauseExit = true;                                         
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(4, SmoothScroll);
                    CaptureFragment.ProfileCaptured += DriverCaptureFragment_ProfileCaptured;
                }
                else if (stage == "3")
                {
                    shouldCauseExit = true;
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(5, SmoothScroll);
                    CarRegFragment.OnCardetailsSaved += CarRegFragment_OnCardetailsSaved;
                }

                else if (stage == "4")
                {
                    shouldCauseExit = true;
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(6, SmoothScroll);
                }
            }
            else
            {
                SetParentFragment();
            }
        }

        private void SetViewPagerAdapter()
        {
            regAdapter = new RegistrationFragmentsAdapter(SupportFragmentManager, Lifecycle);
            
            regAdapter.AddFragments(OnboardingFragment);
            regAdapter.AddFragments(SignInFragment);
            regAdapter.AddFragments(RegisterFragment);
            regAdapter.AddFragments(PartnerFragment);
            regAdapter.AddFragments(CaptureFragment);
            regAdapter.AddFragments(CarRegFragment);
            regAdapter.AddFragments(CarPicsFragment);

            RegViewPager.Adapter = regAdapter;
            
        }

        private void SetParentFragment()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(0, true);
            OnboardingFragment.SignIn += OnboardingFragment_SignIn;
            OnboardingFragment.SignUp += OnboardingFragment_SignUp;
        }

        private void OnboardingFragment_SignIn(object sender, EventArgs e)
        {
            //sign  in fragment
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(1, SmoothScroll);
        }

        private void OnboardingFragment_SignUp(object sender, EventArgs e)
        {
            //sign up fragment
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(2, SmoothScroll);
            RegisterFragment.SignUpSuccess += RegisterFragment_SignUpSuccess;
        }

        private void RegisterFragment_SignUpSuccess(object sender, DriverRegisterFragment.SignUpSuccessArgs e)
        {
            var driver = e.driver;
            if (driver == null)
                return;

            SaveDriverPersonal(driver);
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
            Cab360Drivers.Put("stage_of_registration", RegistrationStage.Registration.ToString());
            SaveDriverToDb(Cab360Drivers);
        }

        private void SaveDriverToDb(HashMap cab360Drivers)
        {
            driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid);
            driverRef.SetValue(cab360Drivers)
                .AddOnSuccessListener(SignUpCompletListener)
                .AddOnFailureListener(SignUpCompletListener);
            SignUpCompletListener.Successful += SignUpCompletListener_Successful; 
            SignUpCompletListener.Failure += SignUpCompletListener_Failure;
        }

        private void SignUpCompletListener_Failure(object sender, EventArgs e)
        {
            
        }

        private void SignUpCompletListener_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            if (driverRef == null)
                driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid);

            driverRef.Child("stage_of_registration").SetValue(RegistrationStage.Partnering.ToString())
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);

            TaskCompletionListener.Successful += TaskCompletionListener_Successful;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure;
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            
        }

        private void TaskCompletionListener_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            //partner fragment
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(3, SmoothScroll);
            PartnerFragment.PartnerSelected += PartnerFragment_PartnerSelected;
        }

        private void PartnerFragment_PartnerSelected(object sender, DriverPartnerFragment.PartnerEventArgs e)
        {
            var isPartner = e.IsPartner;
            switch (isPartner)
            {
                case true:
                    if (driverRef == null)
                        driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid);

                    driverRef.Child("isPartner").SetValue(isPartner.ToString())
                        .AddOnSuccessListener(PartnerStateSavedSuccess)
                        .AddOnFailureListener(PartnerStateSavedSuccess);
                    PartnerStateSavedSuccess.Successful += PartnerStateSavedSuccess_Successful;
                    PartnerStateSavedSuccess.Failure += PartnerStateSavedSuccess_Failure;
                    break;

                case false:
                    
                    break;
            }
        }

        private void PartnerStateSavedSuccess_Failure(object sender, EventArgs e)
        {
            
        }

        private void PartnerStateSavedSuccess_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            if (driverRef == null)
                driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid);

            driverRef.Child("stage_of_registration").SetValue(RegistrationStage.Capturing.ToString())
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful1;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure1;
        }

        private void TaskCompletionListener_Failure1(object sender, EventArgs e)
        {
             
        }

        private void TaskCompletionListener_Successful1(object sender, TaskCompletionListeners.ResultArgs e)
        {

            //driver capture fragment
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(4, SmoothScroll);
            CaptureFragment.ProfileCaptured += DriverCaptureFragment_ProfileCaptured;
        }

        private void DriverCaptureFragment_ProfileCaptured(object sender, EventArgs e)
        {
            if (driverRef == null)
                driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid); 

            driverRef.Child("stage_of_registration").SetValue(RegistrationStage.CarRegistering.ToString())
                    .AddOnSuccessListener(ProfileCapturedSuccess)
                    .AddOnFailureListener(ProfileCapturedSuccess);
            ProfileCapturedSuccess.Successful += ProfileCapturedSuccess_Successful;
            ProfileCapturedSuccess.Failure += ProfileCapturedSuccess_Failure;
        }

        private void ProfileCapturedSuccess_Failure(object sender, EventArgs e)
        {
            
        }

        private void ProfileCapturedSuccess_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(5, SmoothScroll);
            CarRegFragment.OnCardetailsSaved += CarRegFragment_OnCardetailsSaved;
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
                .AddOnSuccessListener(CarCapturedSuccess)
                .AddOnFailureListener(CarCapturedSuccess);
            CarCapturedSuccess.Successful += CarCapturedSuccess_Successful;
            CarCapturedSuccess.Failure += CarCapturedSuccess_Failure;
        }

        private void CarCapturedSuccess_Failure(object sender, EventArgs e)
        {
            
        }

        private void CarCapturedSuccess_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            if (driverRef == null)
                driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid);

            driverRef.Child("stage_of_registration").SetValue(RegistrationStage.CarCapturing.ToString())
                    .AddOnSuccessListener(TaskCompletionListener)
                    .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful2;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure2;
        }

        private void TaskCompletionListener_Failure2(object sender, EventArgs e)
        {
            
        }

        private void TaskCompletionListener_Successful2(object sender, TaskCompletionListeners.ResultArgs e)
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(6, SmoothScroll);
        }

        public override void OnBackPressed()
        {
            
            base.OnBackPressed();
            if(shouldCauseExit == true)
            {
                Finish();
            }
        }
    }
}