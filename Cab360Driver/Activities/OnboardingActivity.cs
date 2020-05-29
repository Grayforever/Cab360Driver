using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Fragments;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using System;


namespace Cab360Driver.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false, Theme ="@style/AppTheme", 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize, 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
        WindowSoftInputMode = SoftInput.AdjustResize| SoftInput.StateHidden)]

    public class OnboardingActivity : FragmentActivity
    {
        private RegistrationFragmentsAdapter regAdapter;
        private ViewPager2 RegViewPager;

        //fragments init
        public OnboardingFragment OnboardingFragment = new OnboardingFragment();
        public DriverSignInFragment SignInFragment = new DriverSignInFragment();
        public DriverRegisterFragment RegisterFragment = new DriverRegisterFragment();
        public DriverCaptureFragment CaptureFragment = new DriverCaptureFragment();
        public DriverPartnerFragment PartnerFragment = new DriverPartnerFragment();
        public CarRegFragment CarRegFragment = new CarRegFragment();
        public CarPicsFragment CarPicsFragment = new CarPicsFragment();
        bool SmoothScroll = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.container_main);

            RegViewPager = FindViewById<ViewPager2>(Resource.Id.reg_viewpager1);
            RegViewPager.Orientation = ViewPager2.OrientationHorizontal;
            RegViewPager.OffscreenPageLimit = 3;
            RegViewPager.UserInputEnabled = false;

            SetViewPagerAdapter();        
            GetStage(AppDataHelper.GetStage());
            RequestPermissions(StringConstants.GetLocationPermissiongroup(), 0);  
        }

        private void GetStage(string stage)
        {
            if (!string.IsNullOrEmpty(stage))
            {
                if (stage == RegistrationStage.Partnering.ToString())
                {
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(3, SmoothScroll);
                    PartnerFragment.PartnerTypeComplete += PartnerFragment_PartnerTypeComplete;
                }
                else if (stage == RegistrationStage.Capturing.ToString())
                {                                         
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(4, SmoothScroll);
                    CaptureFragment.ProfileCaptured += DriverCaptureFragment_ProfileCaptured;
                }
                else if (stage == RegistrationStage.CarRegistering.ToString())
                {
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(5, SmoothScroll);
                    CarRegFragment.CarRegComplete += CarRegFragment_CarRegComplete;
                }

                else if (stage == RegistrationStage.CarCapturing.ToString())
                {
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
            bool status = e.IsCompleted;
            if(status == true)
            {
                regAdapter.NotifyDataSetChanged();
                RegViewPager.SetCurrentItem(3, SmoothScroll);
                PartnerFragment.PartnerTypeComplete += PartnerFragment_PartnerTypeComplete;
            }
        }

        private void PartnerFragment_PartnerTypeComplete(object sender, DriverPartnerFragment.PartnerEventArgs e)
        {
            bool isSuccess = e.IsPartnerComplete;
            if (isSuccess is true)
            {
                regAdapter.NotifyDataSetChanged();
                RegViewPager.SetCurrentItem(4, SmoothScroll);
                CaptureFragment.ProfileCaptured += DriverCaptureFragment_ProfileCaptured; 
            }
        }

        private void DriverCaptureFragment_ProfileCaptured(object sender, EventArgs e)
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(5, SmoothScroll);
            CarRegFragment.CarRegComplete += CarRegFragment_CarRegComplete;
        }

        private void CarRegFragment_CarRegComplete(object sender, EventArgs e)
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(6, SmoothScroll);
            CarPicsFragment.CarCaptureComplete += CarPicsFragment_CarCaptureComplete;
        }

        private void CarPicsFragment_CarCaptureComplete(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }
    }
}