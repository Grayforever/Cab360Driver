using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.BroadcastReceivers;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Fragments;
using Java.Lang;
using System;
using System.Runtime.CompilerServices;

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
        DriverSignInFragment SignInFragment = new DriverSignInFragment();
        DriverRegisterFragment RegisterFragment = new DriverRegisterFragment();
        DriverCaptureFragment CaptureFragment = new DriverCaptureFragment();
        DriverPartnerFragment PartnerFragment = new DriverPartnerFragment();
        CarRegFragment CarRegFragment = new CarRegFragment();
        CarPicsFragment CarPicsFragment = new CarPicsFragment();
        AckFragment AckFragment = new AckFragment();
        private bool isSmoothScroll = false;
        private static FragmentActivity _context;
        private BroadcastReceiver mNetworkReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.container_main);
            var stage = Intent.GetStringExtra("stage");
            _context = this;
            mNetworkReceiver = new NetworkReceiver();
            RegisterNetworkBroadcastForNougat();
            RegViewPager = FindViewById<ViewPager2>(Resource.Id.reg_viewpager1);
            RegViewPager.Orientation = ViewPager2.OrientationHorizontal;
            RegViewPager.OffscreenPageLimit = 4;
            RegViewPager.UserInputEnabled = false;
            SetViewPagerAdapter();
            RequestPermissions(StringConstants.GetLocationPermissiongroup(), 0);
            GetStage(stage);
            
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
                    CarPicsFragment.CarCaptureComplete += CarPicsFragment_CarCaptureComplete;
                }

                else if (stage == RegistrationStage.Agreement.ToString())
                {
                    regAdapter.NotifyDataSetChanged();
                    RegViewPager.SetCurrentItem(7, isSmoothScroll);
                    AckFragment.OnSkip += AckFragment_OnSkip;
                }
            }
            else
            {
                SetParentFrag();
            }
        }

        private void SetParentFrag()
        {
            regAdapter.NotifyDataSetChanged();
            RegViewPager.SetCurrentItem(0, isSmoothScroll);
        }

        private void SetViewPagerAdapter()
        {
            regAdapter = new RegistrationFragmentsAdapter(SupportFragmentManager, Lifecycle);
            regAdapter.AddFragments(new OnboardingFragment(e1 =>
            {
                SetRegisterFrag();
            }, e2 =>
            {
                SetSignInFrag();
            }));
            regAdapter.AddFragments(SignInFragment);
            regAdapter.AddFragments(RegisterFragment);
            regAdapter.AddFragments(PartnerFragment);
            regAdapter.AddFragments(CaptureFragment);
            regAdapter.AddFragments(CarRegFragment);
            regAdapter.AddFragments(CarPicsFragment);
            regAdapter.AddFragments(AckFragment);
            RegViewPager.Adapter = regAdapter;
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

        public static void ShowNoNetDialog(bool val)
        {
            
            if (val != true)
            {
                NoNetBottomSheet noNetBottomSheet = new NoNetBottomSheet(_context);
                noNetBottomSheet.Cancelable = false;
                noNetBottomSheet.Show(_context.SupportFragmentManager, "nonet");
            }
            else
            {
                Toast.MakeText(_context, "Online", ToastLength.Long).Show();
            } 
        }

        private void RegisterNetworkBroadcastForNougat()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                RegisterReceiver(mNetworkReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
            }
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                RegisterReceiver(mNetworkReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
            }
        }

        protected void UnregisterNetworkChanges()
        {
            try
            {
                UnregisterReceiver(mNetworkReceiver);
            }
            catch (IllegalArgumentException e)
            {
                e.PrintStackTrace();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterReceiver(mNetworkReceiver);
        }

        public override void OnBackPressed()
        {
            if(RegViewPager.CurrentItem == 1 || RegViewPager.CurrentItem == 2)
            {
                RegViewPager.SetCurrentItem(0, isSmoothScroll);
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}