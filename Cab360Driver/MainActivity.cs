using Android;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.ViewPager.Widget;
using AndroidX.ViewPager2.Widget;
using Cab360Driver.Activities;
using Cab360Driver.Adapters;
using Cab360Driver.DataModels;
using Cab360Driver.EventListeners;
using Cab360Driver.Fragments;
using Cab360Driver.Helpers;
using CN.Pedant.SweetAlert;
using Google.Android.Material.Badge;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.FloatingActionButton;
using Java.Lang;
using Java.Net;
using System;

namespace Cab360Driver
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false, ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity : AppCompatActivity
    {
        //Buttons
        private FloatingActionButton fabToggleOnline;

        //Views
        ViewPager2 viewpager;
        BottomNavigationView bnve;

        //Fragments
        HomeFragment homeFragment = new HomeFragment();
        RatingsFragment ratingsFragment = new RatingsFragment();
        EarningsFragment earningsFragment = new EarningsFragment();
        AccountFragment accountFragment = new AccountFragment();
        NewRequestFragment requestFoundDialogue;

        //PermissionRequest
        const int RequestID = 0;
        readonly string[] permissionsGroup =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
        };


        //EventListeners
        AvailablityListener availablityListener;
        RideDetailsListener rideDetailsListener;
        NewTripEventListener newTripEventListener;
        WarningEvent1 warningEvent = new WarningEvent1();
        WarningEvent2 startTripEvent = new WarningEvent2();
        private NetStateReceiver netState = new NetStateReceiver();

        //dialogs
        SweetAlertDialog loadingDialog;
        SweetAlertDialog startTripAlert;
        //Map Stuffs
        Android.Locations.Location mLastLocation;
        LatLng mLastLatLng;


        //Flags
        bool availablityStatus;
        bool isBackground;
        bool newRideAssigned;
        string status = "NORMAL"; //REQUESTFOUND, ACCEPTED, ONTRIP

        //Datamodels
        RideDetails newRideDetails;

        //MediaPlayer
        MediaPlayer player;

        //Helpers
        MapFunctionHelper mapHelper;


        ProfileEventListener profileEvent = new ProfileEventListener();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            loadingDialog = new SweetAlertDialog(this, SweetAlertDialog.ProgressType);
            startTripAlert = new SweetAlertDialog(this, SweetAlertDialog.WarningType);

            ConnectViews();
            CheckSpecialPermission();

            profileEvent.Create();
            
        }

        private void ShowOfflineDialog(bool IsConnected)
        {
            if (!IsConnected)
            {
                new SweetAlertDialog(this, SweetAlertDialog.ErrorType)
                    .SetTitleText("Network errror")
                    .SetContentText("You are not connected to the internet")
                    .SetConfirmText("OK")
                    .SetConfirmClickListener(null)
                    .Show();
            }
            else
            {
                return;
            }
            
        }

        protected override void OnStart()
        {
            base.OnStart();
            profileEvent.UserFoundEvent += ProfileEvent_UserFoundEvent;
            profileEvent.UserNotFoundEvent += ProfileEvent_UserNotFoundEvent;
        }

        private void ProfileEvent_UserNotFoundEvent(object sender, EventArgs e)
        {
            new SweetAlertDialog(this, SweetAlertDialog.WarningType)
                .SetTitleText("Oops...")
                .SetContentText("Something went wrong. Please check your network connection!")
                .SetConfirmText("Sure")
                .SetConfirmClickListener(null)
                .Show();
        }

        private void ProfileEvent_UserFoundEvent(object sender, EventArgs e)
        {
            Toast.MakeText(this, $"Welcome, {AppDataHelper.GetFirstName()}", ToastLength.Long).Show();
        }

        void ShowProgressDialogue()
        {
            loadingDialog.ProgressHelper.BarColor = Resource.Color.colorAccent;
            loadingDialog.SetTitleText("Loading");
            loadingDialog.SetCancelable(false);
            loadingDialog.Show();
        }

        void CloseProgressDialogue()
        {
            if (loadingDialog.IsShowing)
            {
                loadingDialog.DismissWithAnimation();
            }
        }


        void ConnectViews()
        {
            
            fabToggleOnline = FindViewById<FloatingActionButton>(Resource.Id.fab_toggle_online);
            fabToggleOnline.Click += FabToggleOnline_Click;

            bnve = (BottomNavigationView)FindViewById(Resource.Id.bnve);
            BadgeDrawable badge = bnve.GetOrCreateBadge(Resource.Menu.bottomnav);
            badge.SetVisible(true);


            bnve.NavigationItemSelected += Bnve_NavigationItemSelected1;

            viewpager = (ViewPager2)FindViewById(Resource.Id.viewpager);
            viewpager.Orientation = ViewPager2.OrientationHorizontal;
            viewpager.OffscreenPageLimit = 3;
            viewpager.UserInputEnabled = false;

            SetupViewPager();

            homeFragment.CurrentLocation += HomeFragment_CurrentLocation;
            homeFragment.TripActionArrived += HomeFragment_TripActionArrived;
            homeFragment.CallRider += HomeFragment_CallRider;
            homeFragment.Navigate += HomeFragment_Navigate;
            homeFragment.TripActionStartTrip += HomeFragment_TripActionStartTrip;
            homeFragment.TripActionEndTrip += HomeFragment_TripActionEndTrip;

            
        }

        private void Bnve_NavigationItemSelected1(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            var itemID = e.Item.ItemId;
            switch (itemID)
            {
                case Resource.Id.action_earning:
                    viewpager.SetCurrentItem(1, true);
                    break;

                case Resource.Id.action_home:
                    viewpager.SetCurrentItem(0, true);
                    break;

                case Resource.Id.action_rating:
                    viewpager.SetCurrentItem(2, true);
                    break;

                case Resource.Id.action_account:
                    viewpager.SetCurrentItem(3, true);
                    break;

                default:
                    return;

            }
        }

        private void FabToggleOnline_Click(object sender, EventArgs e)
        {
            if (!CheckSpecialPermission())
            {
                return;
            }

            if (availablityStatus)
            {
                new SweetAlertDialog(this, SweetAlertDialog.WarningType)
                    .SetTitleText("Go offline")
                    .SetContentText("You will not receive any ride request. Continue?")
                    .SetConfirmText("Yes")
                    .SetCancelText("No")
                    .SetConfirmClickListener(warningEvent)
                    .Show();
                warningEvent.OnConfirmClick += WarningEvent_OnConfirmClick;
            }
            else
            {
                availablityStatus = true;
                homeFragment.GoOnline();
                fabToggleOnline.SetBackgroundColor(Color.LightGreen);
            }
        }

        private void WarningEvent_OnConfirmClick(object sender, EventArgs e)
        {
            homeFragment.GoOffline();
            fabToggleOnline.SetBackgroundColor(Color.OrangeRed);
            availablityStatus = false;
            TakeDriverOffline();
        }

        public async void HomeFragment_TripActionEndTrip(object sender, EventArgs e)
        {
            //Reset app
            status = "NORMAL";
            homeFragment.ResetAfterTrip();


            ShowProgressDialogue();

            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            double fares = await mapHelper.CalculateFares(pickupLatLng, mLastLatLng);
            CloseProgressDialogue();

            newTripEventListener.EndTrip(fares);
            newTripEventListener = null;

            CollectPaymentFragment collectPaymentFragment = new CollectPaymentFragment(fares);
            collectPaymentFragment.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            collectPaymentFragment.Show(trans, "pay");
            collectPaymentFragment.PaymentCollected += (o, u) =>
            {
                collectPaymentFragment.Dismiss();
            };

            availablityListener.ReActivate();

        }

        public void HomeFragment_TripActionStartTrip(object sender, EventArgs e)
        {
            
            startTripAlert.SetTitleText("Start Trip");
            startTripAlert.SetContentText("Sure to start trip?");
            startTripAlert.SetCancelText("No");
            startTripAlert.SetConfirmText("Yes");
            startTripAlert.SetConfirmClickListener(startTripEvent);
            startTripAlert.Show();

            startTripEvent.OnConfirmClick += StartTripEvent_OnConfirmClick;
        }

        private void StartTripEvent_OnConfirmClick(object sender, EventArgs e)
        {
            status = "ONTRIP";

            // Update Rider that Driver has started the trip
            newTripEventListener.UpdateStatus("ontrip");
        }

        private void HomeFragment_Navigate(object sender, EventArgs e)
        {
            if (newRideDetails == null)
                return;

            string uriString = "";

            if (status == "ACCEPTED")
            {
                uriString = "google.navigation:q=" + newRideDetails.PickupLat.ToString() + "," + newRideDetails.PickupLng.ToString();
            }
            else
            {
                uriString = "google.navigation:q=" + newRideDetails.DestinationLat.ToString() + "," + newRideDetails.DestinationLng.ToString();
            }

            Android.Net.Uri googleMapIntentUri = Android.Net.Uri.Parse(uriString);
            Intent mapIntent = new Intent(Intent.ActionView, googleMapIntentUri);
            mapIntent.SetPackage("com.google.android.apps.maps");

            try
            {
                StartActivity(mapIntent);
            }
            catch
            {
                Toast.MakeText(this, "Google Map is not Installed on this device", ToastLength.Short).Show();
            }
        }


        void HomeFragment_CallRider(object sender, EventArgs e)
        {
            if (newRideDetails == null)
                return;

            var uri = Android.Net.Uri.Parse("tel:" + newRideDetails.RiderPhone);
            Intent intent = new Intent(Intent.ActionDial, uri);
            StartActivity(intent);
        }

        async void HomeFragment_TripActionArrived(object sender, EventArgs e)
        {
            //Notifies Rider that Driver has arrived
            newTripEventListener.UpdateStatus("arrived");
            status = "ARRIVED";

            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            LatLng destinationLatLng = new LatLng(newRideDetails.DestinationLat, newRideDetails.DestinationLng);

            ShowProgressDialogue();
            string directionJson = await mapHelper.GetDirectionJsonAsync(pickupLatLng, destinationLatLng);
            CloseProgressDialogue();

            //Clear Map
            homeFragment.mainMap.Clear();
            mapHelper.DrawTripToDestination(directionJson);
        }

        void HomeFragment_CurrentLocation(object sender, LocationCallbackHelper.OnLocationCaptionEventArgs e)
        {
            mLastLocation = e.Location;
            mLastLatLng = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);

            if (availablityListener != null)
            {
                availablityListener.UpdateLocation(mLastLocation);
            }

            if (availablityStatus && availablityListener == null)
            {
                TakeDriverOnline();
            }

            if (status == "ACCEPTED")
            {
                //Update and Animate driver movement to pick up lOcation
                LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
                mapHelper.UpdateMovement(mLastLatLng, pickupLatLng, "Rider");

                //Updates Location in rideRequest Table, so that Rider can receive Updates
                newTripEventListener.UpdateLocation(mLastLocation);

            }
            else if (status == "ARRIVED")
            {
                newTripEventListener.UpdateLocation(mLastLocation);
            }
            else if (status == "ONTRIP")
            {
                //Update and animate driver movement to Destination
                LatLng destinationLatLng = new LatLng(newRideDetails.DestinationLat, newRideDetails.DestinationLng);
                mapHelper.UpdateMovement(mLastLatLng, destinationLatLng, "Destination");

                //Update Location on firebase
                newTripEventListener.UpdateLocation(mLastLocation);
            }


        }

        private void TakeDriverOnline()
        {
            if(mLastLocation != null)
            {
                availablityListener = new AvailablityListener();
                availablityListener.Create(mLastLocation);
                availablityListener.RideAssigned += AvailablityListener_RideAssigned;
                availablityListener.RideTimedOut += AvailablityListener_RideTimedOut;
                availablityListener.RideCancelled += AvailablityListener_RideCancelled;
            }
            else
            {
                return;
            }
        }

        void TakeDriverOffline()
        {
            if(availablityListener != null)
            {
                availablityListener.RemoveListener();
                availablityListener = null;
            }
            else
            {
                return;
            }

        }

        void AvailablityListener_RideAssigned(object sender, AvailablityListener.RideAssignedIDEventArgs e)
        {
            var rideID = e.RideId;
            if(!string.IsNullOrEmpty(rideID) && !string.IsNullOrWhiteSpace(rideID))
            {
                //Get Details
                rideDetailsListener = new RideDetailsListener();
                rideDetailsListener.Create(rideID);
                rideDetailsListener.RideDetailsFound += RideDetailsListener_RideDetailsFound;
                rideDetailsListener.RideDetailsNotFound += RideDetailsListener_RideDetailsNotFound;
            }
            else
            {
                return;
            }
            

        }

        void RideDetailsListener_RideDetailsNotFound(object sender, EventArgs e)
        {

        }

        void CreateNewRequestDialogue()
        {
            requestFoundDialogue = new NewRequestFragment(newRideDetails.PickupAddress, newRideDetails.DestinationAddress);
            requestFoundDialogue.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            requestFoundDialogue.Show(trans, "Request");

            //Play Alert
            player = MediaPlayer.Create(this, Resource.Raw.alert);
            player.Start();

            requestFoundDialogue.RideRejected += RequestFoundDialogue_RideRejected;
            requestFoundDialogue.RideAccepted += RequestFoundDialogue_RideAccepted;
        }

        async void RequestFoundDialogue_RideAccepted(object sender, EventArgs e)
        {
            newTripEventListener = new NewTripEventListener(newRideDetails.RideId, mLastLocation);
            newTripEventListener.Create();

            status = "ACCEPTED";

            //Stop Alert
            if (player != null)
            {
                player.Stop();
                player = null;
            }

            //Dissmiss Dialogue
            if (requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
            }

            homeFragment.CreateTrip(newRideDetails.RiderName);
            mapHelper = new MapFunctionHelper(Resources.GetString(Resource.String.mapKey), homeFragment.mainMap);
            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);

            ShowProgressDialogue();
            string directionJson = await mapHelper.GetDirectionJsonAsync(mLastLatLng, pickupLatLng);
            

            mapHelper.DrawTripOnMap(directionJson);
            CloseProgressDialogue();
        }

        void RequestFoundDialogue_RideRejected(object sender, EventArgs e)
        {
            //Stop Alert
            if (player != null)
            {
                player.Stop();
                player = null;
            }

            //Dissmiss Dialogue
            if (requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
            }

            availablityListener.ReActivate();

            //Do other stuff
        }

        void RideDetailsListener_RideDetailsFound(object sender, RideDetailsListener.RideDetailsEventArgs e)
        {
            if (status != "NORMAL")
            {
                return;
            }
            newRideDetails = e.RideDetails;

            if (!isBackground)
            {
                CreateNewRequestDialogue();
            }
            else
            {
                newRideAssigned = true;
                NotificationHelper notificationHelper = new NotificationHelper();
                if ((int)Build.VERSION.SdkInt >= 26)
                {
                    notificationHelper.NotifyVersion26(this, Resources, (NotificationManager)GetSystemService(NotificationService));
                }
            }
        }

        void AvailablityListener_RideTimedOut(object sender, EventArgs e)
        {
            if (requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
                player.Stop();
                player = null;
            }

            Toast.MakeText(this, "New trip Timeout", ToastLength.Short).Show();
            availablityListener.ReActivate();
        }

        void AvailablityListener_RideCancelled(object sender, EventArgs e)
        {
            if (requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
                player.Stop();
                player = null;
            }

            Toast.MakeText(this, "New trip was cancelled", ToastLength.Short).Show();
            availablityListener.ReActivate();
        }

        private void SetupViewPager()
        {
            ViewPagerAdapter adapter = new ViewPagerAdapter(SupportFragmentManager, Lifecycle);


            adapter.AddFragment(homeFragment, "Home");
            adapter.AddFragment(earningsFragment, "Earnings");
            adapter.AddFragment(ratingsFragment, "Rating");
            adapter.AddFragment(accountFragment, "Account");
            
            viewpager.Adapter = adapter;
        }

        bool CheckSpecialPermission()
        {
            bool permissionGranted = false;
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted &&
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(permissionsGroup, RequestID);
            }
            else
            {
                permissionGranted = true;
            }

            return permissionGranted;
        }

        public static bool IsOnline1()
        {
            try
            {
                Runtime runtime = Runtime.GetRuntime();
                Java.Lang.Process IpProcess = runtime.Exec("/system/bin/ping -c 1 8.8.8.8");
                int exitValue = IpProcess.WaitFor();
                return (exitValue == 0);
            }
            catch (InvalidOperationException ioe)
            {

                throw;
            }
            catch(InterruptedException ie)
            {
                
            }
            return false;
        }

        protected override void OnPause()
        {
            base.OnPause();
            isBackground = true;
        }

        protected override void OnResume()
        {
            base.OnResume();
            isBackground = false;
            if (newRideAssigned)
            {
                CreateNewRequestDialogue();
                newRideAssigned = false;
            }
        }
    }
}