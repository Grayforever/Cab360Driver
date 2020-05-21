using Android;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Media;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.DataModels;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Fragments;
using Cab360Driver.Helpers;
using CN.Pedant.SweetAlert;
using Google.Android.Material.Badge;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.FloatingActionButton;
using Java.Lang;
using System;

namespace Cab360Driver
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity : FragmentActivity, IRunnable
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
        private const int RequestID = 0;
        
        //EventListeners
        AvailablityListener availablityListener;
        RideDetailsListener rideDetailsListener;
        NewTripEventListener newTripEventListener;
        WarningEvent1 warningEvent = new WarningEvent1();
        WarningEvent2 startTripEvent = new WarningEvent2();

        //Map Stuffs
        Android.Locations.Location mLastLocation;
        LatLng mLastLatLng;


        //Flags
        bool availablityStatus;
        bool isBackground;
        bool newRideAssigned;
        private RideStatusEnum statusEnum;
        private int rotation = 180;

        //Datamodels
        private RideDetails newRideDetails;

        //MediaPlayer
        private MediaPlayer player;

        //Helpers
        private MapFunctionHelper mapHelper;


        private ProfileEventListener profileEvent = new ProfileEventListener();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            ConnectViews();
            CheckSpecialPermission();
            profileEvent.Create();
            statusEnum = RideStatusEnum.Normal;
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
            Log.Debug("username: ", AppDataHelper.GetFirstName());
        }

        private void ConnectViews()
        {
            fabToggleOnline = FindViewById<FloatingActionButton>(Resource.Id.fab_toggle_online);
            fabToggleOnline.Click += FabToggleOnline_Click;

            bnve = (BottomNavigationView)FindViewById(Resource.Id.bnve);
            BadgeDrawable badge = bnve.GetOrCreateBadge(Resource.Menu.bottomnav);
            badge.SetVisible(true);
            bnve.NavigationItemSelected += Bnve_NavigationItemSelected1;

            viewpager = (ViewPager2)FindViewById(Resource.Id.viewpager);
            viewpager.OffscreenPageLimit = 3;
            viewpager.UserInputEnabled = false;
            //viewpager.SetPageTransformer(new CrossfadeTransformer());
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
                    viewpager.SetCurrentItem(1, false);
                    break;

                case Resource.Id.action_home:
                    viewpager.SetCurrentItem(0, false);
                    break;

                case Resource.Id.action_rating:
                    viewpager.SetCurrentItem(2, false);
                    break;

                case Resource.Id.action_account:
                    viewpager.SetCurrentItem(3, false);
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
            }

            fabToggleOnline.Animate()
                .RotationBy(rotation)
                .SetDuration(100)
                .ScaleX(1.1f)
                .ScaleY(1.1f)
                .WithEndAction(this)
                .Start();
        }

        private void WarningEvent_OnConfirmClick(object sender, EventArgs e)
        {
            homeFragment.GoOffline();
            availablityStatus = false;
            TakeDriverOffline();
            
        }

        public void Run()
        {
            fabToggleOnline.SetImageDrawable(GetDrawable(Resource.Drawable.ic_car_online));
            fabToggleOnline.Animate()
                .RotationBy(rotation)
                .SetDuration(100)
                .ScaleX(1)
                .ScaleY(1)
                .Start();
        }

        public async void HomeFragment_TripActionEndTrip(object sender, EventArgs e)
        {
            homeFragment.ResetAfterTrip();
            statusEnum = RideStatusEnum.Normal;

            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            double fares = await mapHelper.CalculateFares(pickupLatLng, mLastLatLng);
            newTripEventListener.EndTrip(fares);
            newTripEventListener = null;
            ShowFareDialog(fares);
            availablityListener.ReActivate();
        }

        private void ShowFareDialog(double fares)
        {
            CollectPaymentFragment collectPaymentFragment = new CollectPaymentFragment(fares);
            collectPaymentFragment.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            collectPaymentFragment.Show(trans, "pay");
            collectPaymentFragment.PaymentCollected += (o, u) =>
            {
                collectPaymentFragment.Dismiss();
            };
        }

        public void HomeFragment_TripActionStartTrip(object sender, EventArgs e)
        {
            
            //startTripAlert.SetTitleText("Start Trip");
            //startTripAlert.SetContentText("Sure to start trip?");
            //startTripAlert.SetCancelText("No");
            //startTripAlert.SetConfirmText("Yes");
            //startTripAlert.SetConfirmClickListener(startTripEvent);
            //startTripAlert.Show();

            //startTripEvent.OnConfirmClick += StartTripEvent_OnConfirmClick;
        }

        private void StartTripEvent_OnConfirmClick(object sender, EventArgs e)
        {
            statusEnum = RideStatusEnum.Ontrip;
            newTripEventListener.UpdateStatus(statusEnum);
        }

        private void HomeFragment_Navigate(object sender, EventArgs e)
        {
            try
            {
                if (newRideDetails != null)
                {
                    string uriString = statusEnum == RideStatusEnum.Accepted
                        ? NavUriString(newRideDetails.PickupLat, newRideDetails.PickupLng)
                        : NavUriString(newRideDetails.DestinationLat, newRideDetails.DestinationLng);
                    OpenGoogleMap(uriString);
                }
                else
                {
                    return;
                }
            }
            catch (System.Exception exception)
            {
                Toast.MakeText(this, exception.Message, ToastLength.Short).Show();
            }
        }

        private void OpenGoogleMap(string uriString)
        {
            Android.Net.Uri googleMapIntentUri = Android.Net.Uri.Parse(uriString);
            Intent mapIntent = new Intent(Intent.ActionView, googleMapIntentUri);
            mapIntent.SetPackage("com.google.android.apps.maps");
            StartActivity(mapIntent);
        }

        private string NavUriString(double lat, double lng)
        {
            return $"{StringConstants.GetNavigateBaseGateway()}{lat},{lng}";
        }

        private void HomeFragment_CallRider(object sender, EventArgs e)
        {
            if (newRideDetails == null)
                return;

            var uri = Android.Net.Uri.Parse($"tel:{newRideDetails.RiderPhone}");
            Intent intent = new Intent(Intent.ActionDial, uri);
            StartActivity(intent);
        }

        private async void HomeFragment_TripActionArrived(object sender, EventArgs e)
        {
            //Notifies Rider that Driver has arrived
            statusEnum = RideStatusEnum.Arrived;
            newTripEventListener.UpdateStatus(statusEnum);

            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            LatLng destinationLatLng = new LatLng(newRideDetails.DestinationLat, newRideDetails.DestinationLng);

            string directionJson = await mapHelper.GetDirectionJsonAsync(pickupLatLng, destinationLatLng);

            //Clear Map
            homeFragment.mainMap.Clear();
            mapHelper.DrawTripToDestination(directionJson);
        }

        private void HomeFragment_CurrentLocation(object sender, LocationCallbackHelper.OnLocationCaptionEventArgs e)
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

            switch (statusEnum)
            {
                case RideStatusEnum.Accepted:
                    {
                        //Update and Animate driver movement to pick up lOcation
                        LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
                        mapHelper.UpdateMovement(mLastLatLng, pickupLatLng, "Rider");

                        //Updates Location in rideRequest Table, so that Rider can receive Updates
                        newTripEventListener.UpdateLocation(mLastLocation);
                        break;
                    }

                case RideStatusEnum.Arrived:
                    newTripEventListener.UpdateLocation(mLastLocation);
                    break;
                case RideStatusEnum.Ontrip:
                    {
                        //Update and animate driver movement to Destination
                        LatLng destinationLatLng = new LatLng(newRideDetails.DestinationLat, newRideDetails.DestinationLng);
                        mapHelper.UpdateMovement(mLastLatLng, destinationLatLng, "Destination");

                        //Update Location on firebase
                        newTripEventListener.UpdateLocation(mLastLocation);
                        break;
                    }

            }
        }

        private void TakeDriverOnline()
        {
            if(mLastLocation != null)
            {
                availablityListener = new AvailablityListener(this);
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

        

        private void TakeDriverOffline()
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

        private void AvailablityListener_RideTimedOut(object sender, AvailablityListener.TimeoutMessageArgs e)
        {
            if (requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
                player.Stop();
                player = null;
            }

            Toast.MakeText(this, e.Message, ToastLength.Short).Show();
            availablityListener.ReActivate();
        }

        private void AvailablityListener_RideAssigned(object sender, AvailablityListener.RideAssignedIDEventArgs e)
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

        private void RideDetailsListener_RideDetailsNotFound(object sender, EventArgs e)
        {

        }

        private void CreateNewRequestDialogue()
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

        private async void RequestFoundDialogue_RideAccepted(object sender, EventArgs e)
        {
            newTripEventListener = new NewTripEventListener(newRideDetails.RideId, mLastLocation);
            newTripEventListener.Create();

            statusEnum = RideStatusEnum.Accepted;

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
            mapHelper = new MapFunctionHelper(homeFragment.mainMap);
            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            string directionJson = await mapHelper.GetDirectionJsonAsync(mLastLatLng, pickupLatLng);

            mapHelper.DrawTripOnMap(directionJson);
        }

        private void RequestFoundDialogue_RideRejected(object sender, EventArgs e)
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

        private void RideDetailsListener_RideDetailsFound(object sender, RideDetailsListener.RideDetailsEventArgs e)
        {
            if (statusEnum != RideStatusEnum.Normal)
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

        private void AvailablityListener_RideCancelled(object sender, EventArgs e)
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

        private bool CheckSpecialPermission()
        {
            bool permissionGranted = false;
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted &&
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(StringConstants.GetLocationPermissiongroup(), RequestID);
            }
            else
            {
                permissionGranted = true;
            }

            return permissionGranted;
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