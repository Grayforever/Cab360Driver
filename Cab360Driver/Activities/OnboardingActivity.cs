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

        private OnboardingFragment OnboardingFragment = new OnboardingFragment();
        private DriverSignInFragment SignInFragment = new DriverSignInFragment();
        private DriverRegisterFragment RegisterFragment = new DriverRegisterFragment();
        private DriverCaptureFragment CaptureFragment = new DriverCaptureFragment();
        private DriverPartnerFragment PartnerFragment = new DriverPartnerFragment();
        private CarRegFragment CarRegFragment = new CarRegFragment();
        private CarPicsFragment CarPicsFragment = new CarPicsFragment();
        private AckFragment AckFragment = new AckFragment();
        private bool isSmoothScroll = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.container_main);
            
            RegViewPager = FindViewById<ViewPager2>(Resource.Id.reg_viewpager1);
            RegViewPager.Orientation = ViewPager2.OrientationHorizontal;
            RegViewPager.OffscreenPageLimit = 4;
            RegViewPager.UserInputEnabled = false;
            GetStage(AppDataHelper.GetStage());
            SetViewPagerAdapter();                
            RequestPermissions(StringConstants.GetLocationPermissiongroup(), 0);  
        }

        private void GetStage(string stage)
        {
            if (!string.IsNullOrEmpty(stage))
            {
                if (stage == RegistrationStage.Partnering.ToString())
                {
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(3, isSmoothScroll);
                    PartnerFragment.PartnerTypeComplete += PartnerFragment_PartnerTypeComplete;
                }
                else if (stage == RegistrationStage.Capturing.ToString())
                {                                         
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(4, isSmoothScroll);
                    CaptureFragment.ProfileCaptured += DriverCaptureFragment_ProfileCaptured;
                }
                else if (stage == RegistrationStage.CarRegistering.ToString())
                {
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(5, isSmoothScroll);
                    CarRegFragment.CarRegComplete += CarRegFragment_CarRegComplete;
                }

                else if (stage == RegistrationStage.CarCapturing.ToString())
                {
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(6, isSmoothScroll);
                }

                else if (stage == RegistrationStage.Agreement.ToString())
                {
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(7, isSmoothScroll);
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

        private void SetRegisterFrag()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(2, isSmoothScroll);
            RegisterFragment.SignUpSuccess += RegisterFragment_SignUpSuccess;

        }

        private void SetSignInFrag()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(1, isSmoothScroll);
        }

        private void SetPartnerFrag()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(3, isSmoothScroll);
            PartnerFragment.PartnerTypeComplete += PartnerFragment_PartnerTypeComplete;
        }

        private void SetCaptureFrag()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(4, isSmoothScroll);
            CaptureFragment.ProfileCaptured += DriverCaptureFragment_ProfileCaptured;
        }

        private void SetCarCaptFrag()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(6, isSmoothScroll);
            CarPicsFragment.CarCaptureComplete += CarPicsFragment_CarCaptureComplete;
        }

        private void SetCarRegFrag()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(5, isSmoothScroll);
            CarRegFragment.CarRegComplete += CarRegFragment_CarRegComplete;
        }

        private void SetAckFrag()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(7, isSmoothScroll);
            AckFragment.OnSkip += AckFragment_OnSkip;
        }

        private void AckFragment_OnSkip(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(intent);
        }

        private void OnboardingFragment_SignIn(object sender, EventArgs e)
        {
            SetSignInFrag();
        }

        private void OnboardingFragment_SignUp(object sender, EventArgs e)
        {
            SetRegisterFrag();
        }

        private void RegisterFragment_SignUpSuccess(object sender, DriverRegisterFragment.SignUpSuccessArgs e)
        {
            bool status = e.IsCompleted;
            if(status == true)
            {
                SetPartnerFrag();
            }
        }

        private void PartnerFragment_PartnerTypeComplete(object sender, DriverPartnerFragment.PartnerEventArgs e)
        {
            bool isSuccess = e.IsPartnerComplete;
            if (isSuccess is true)
            {
                SetCaptureFrag();
            }
        }

        private void DriverCaptureFragment_ProfileCaptured(object sender, EventArgs e)
        {
            SetCarRegFrag();
        }

        private void CarRegFragment_CarRegComplete(object sender, EventArgs e)
        {
            SetCarCaptFrag();
        }

        private void CarPicsFragment_CarCaptureComplete(object sender, EventArgs e)
        {
            SetAckFrag();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }
    }
}