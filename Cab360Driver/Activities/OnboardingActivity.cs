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
using Cab360Driver.EventListeners;
using Cab360Driver.Fragments;
using CN.Pedant.SweetAlert;
using Java.Lang;
using System;

namespace Cab360Driver.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = false, Theme = "@style/AppTheme", 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize, 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
        WindowSoftInputMode = SoftInput.AdjustResize| SoftInput.StateHidden)]

    public class OnboardingActivity : FragmentActivity
    {
        private RegistrationFragmentsAdapter regAdapter;
        private ViewPager2 RegViewPager;
        private ProgressBar progressBar;
        DriverSignInFragment SignInFragment = new DriverSignInFragment();
        DriverRegisterFragment RegisterFragment = new DriverRegisterFragment();
        DriverCaptureFragment CaptureFragment = new DriverCaptureFragment();
        DriverPartnerFragment PartnerFragment = new DriverPartnerFragment();
        CarRegFragment CarRegFragment = new CarRegFragment();
        AckFragment AckFragment = new AckFragment();
        private bool isSmoothScroll = false;
        private static FragmentActivity _context;
        private BroadcastReceiver mNetworkReceiver;
        public static SweetAlertDialog loadingDialog = null;
        public static SweetAlertDialog errorDialog = null;
        public static NoNetBottomSheet noNetBottomSheet = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.container_main);
            InitControls();
            GetStage(base.Intent.GetStringExtra("stage"));
            mNetworkReceiver = new NetworkReceiver();
            RegisterNetworkBroadcastForNougat();
            _context = this;
        }

        private void InitControls()
        {
            RegViewPager = FindViewById<ViewPager2>(Resource.Id.reg_viewpager1);
            progressBar = FindViewById<ProgressBar>(Resource.Id.cont_viewpager);
            regAdapter = new RegistrationFragmentsAdapter(SupportFragmentManager, Lifecycle);
            RegViewPager.Orientation = ViewPager2.OrientationHorizontal;
            RegViewPager.OffscreenPageLimit = 4;
            RegViewPager.UserInputEnabled = false;

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
            regAdapter.AddFragments(AckFragment);
            RegViewPager.Adapter = regAdapter;
        }


        //router
        private void GetStage(string stage)
        {
            if (!string.IsNullOrEmpty(stage))
            {
                if (stage == RegistrationStage.Partnering.ToString())
                {
                    SetPartnerFrag();
                }
                else if (stage == RegistrationStage.Capturing.ToString())
                {
                    SetCaptureFrag();
                }
                else if (stage == RegistrationStage.CarRegistering.ToString())
                {
                    SetCarRegFrag();
                }

                else if (stage == RegistrationStage.Agreement.ToString())
                {
                    SetAckFrag();
                }
            }
            else
            {
                SetParentFrag();
            }
        }

        private void SetParentFrag()
        {
            RegViewPager.SetCurrentItem(0, isSmoothScroll);
        }

        private void SetSignInFrag()
        {
            RegViewPager.SetCurrentItem(1, isSmoothScroll);
            SignInFragment.onRegUncompleteListener += SignInFragment_onRegUncompleteListener;
        }

        private void SignInFragment_onRegUncompleteListener(object sender, DriverSignInFragment.RegUncompleteArgs e)
        {
            GetStage(e.StageReached);
        }

        private void SetRegisterFrag()
        {
            RegViewPager.SetCurrentItem(2, isSmoothScroll);
            RegisterFragment.SignUpSuccess += RegisterFragment_SignUpSuccess;
            RegisterFragment.OnEmailExistsListener += RegisterFragment_OnEmailExistsListener;
        }

        private void RegisterFragment_OnEmailExistsListener(object sender, EventArgs e)
        {
            SetSignInFrag();
        }

        private void SetPartnerFrag()
        {
            
            RegViewPager.SetCurrentItem(3, isSmoothScroll);
            PartnerFragment.PartnerTypeComplete += PartnerFragment_PartnerTypeComplete;
        }

        private void SetCaptureFrag()
        {
            RegViewPager.SetCurrentItem(4, isSmoothScroll);
            CaptureFragment.ProfileCaptured += DriverCaptureFragment_ProfileCaptured;
        }

        private void SetCarRegFrag()
        {
            RegViewPager.SetCurrentItem(5, isSmoothScroll);
            CarRegFragment.CarRegComplete += CarRegFragment_CarRegComplete;
        }

        private void SetAckFrag()
        {
            
            RegViewPager.SetCurrentItem(6, isSmoothScroll);
        }

        private void RegisterFragment_SignUpSuccess(object sender, DriverRegisterFragment.SignUpSuccessArgs e)
        {
            if (e.IsCompleted == true)
            {
                SetPartnerFrag();
            }
            else
            {
                return;
            }
        }

        private void PartnerFragment_PartnerTypeComplete(object sender, DriverPartnerFragment.PartnerEventArgs e)
        {
            if (e.IsPartnerComplete == true)
            {
                SetCaptureFrag();
            }
            else
            {
                return;
            }
        }

        private void DriverCaptureFragment_ProfileCaptured(object sender, EventArgs e)
        {
            SetCarRegFrag();
        }

        private void CarRegFragment_CarRegComplete(object sender, EventArgs e)
        {
            SetAckFrag();
        }

        //custom
        public static void ShowNoNetDialog(bool val)
        {
            if (val != true)
            {
                noNetBottomSheet = new NoNetBottomSheet(_context);
                noNetBottomSheet.Cancelable = false;
                AndroidX.Fragment.App.FragmentTransaction ft = _context.SupportFragmentManager.BeginTransaction();
                ft.Add(noNetBottomSheet, "no_net");
                ft.CommitAllowingStateLoss();
            }
            else
            {
                return;
            }
        }

        public static void ShowProgressDialog()
        {
            loadingDialog = new SweetAlertDialog(_context, SweetAlertDialog.ProgressType);
            loadingDialog.SetCancelable(false);
            loadingDialog.SetTitleText("Loading");
            loadingDialog.ShowCancelButton(false);
            loadingDialog.Show();
        }

        public static void CloseProgressDialog()
        {
            if (loadingDialog == null)
            {
                return;
            }

            if (loadingDialog.IsShowing)
            {
                loadingDialog.DismissWithAnimation();
                loadingDialog = null;
            }
        }

        public static void ShowErrorDialog(string errorMessage)
        {
            errorDialog = new SweetAlertDialog(_context, SweetAlertDialog.ErrorType);
            errorDialog.SetCancelable(false);
            errorDialog.SetTitleText("Oops...");
            errorDialog.SetContentText(errorMessage);
            errorDialog.SetConfirmText("OK");
            errorDialog.ShowCancelButton(false);
            errorDialog.SetConfirmClickListener(new SweetConfirmClick(d => 
            {
                errorDialog.DismissWithAnimation();
                errorDialog = null;
             
            }));
            errorDialog.Show();
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

        //overrides
        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterReceiver(mNetworkReceiver);
        }

        public override void OnBackPressed()
        {
            switch (RegViewPager.CurrentItem)
            {
                case 1:
                case 2:
                    RegViewPager.SetCurrentItem(0, isSmoothScroll);
                    break;
                default:
                    Finish();
                    break;
            }
        }
    }
}