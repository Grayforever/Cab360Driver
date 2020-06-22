using Android;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.BroadcastReceivers;
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
    public class MainActivity : FragmentActivity
    {
        //Buttons
        private FloatingActionButton fabToggleOnline;

        //Views
        private ViewPager2 viewpager;
        private BottomNavigationView bnve;

        //Fragments
        private HomeFragment homeFragment = new HomeFragment();
        private RatingsFragment ratingsFragment = new RatingsFragment();
        private EarningsFragment earningsFragment = new EarningsFragment();
        private AccountFragment accountFragment = new AccountFragment();
        private NewRequestFragment newRideDialog;

        //PermissionRequest
        private const int RequestID = 0;
        private LocationReceiver _locationReceiver;
        
        //EventListeners
        AvailablityListener availablityListener;
        RideDetailsListener rideDetailsListener;
        NewTripEventListener newTripEventListener;

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
        private static FragmentActivity _this;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            ConnectViews();
            SetupViewPager();
            _this = this;
            
            statusEnum = RideStatusEnum.Normal;
        }

        protected override void OnStart()
        {
            base.OnStart();
            player = MediaPlayer.Create(this, Resource.Raw.alert);
            _locationReceiver = new LocationReceiver();
            mapHelper = new MapFunctionHelper(homeFragment.mainMap);
            CheckSpecialPermission();
        }

        private void ConnectViews()
        {
            fabToggleOnline = FindViewById<FloatingActionButton>(Resource.Id.fab_toggle_online);
            fabToggleOnline.Click += (s2, e2) =>
            {
                if (!CheckSpecialPermission())
                {
                    return;
                }

                if (availablityStatus)
                {
                    var alert2 = new SweetAlertDialog(this, SweetAlertDialog.WarningType);
                    alert2.SetTitleText("Go offline");
                    alert2.SetContentText("You will not receive any ride request. Continue?");
                    alert2.SetConfirmText("Yes");
                    alert2.SetCancelText("No");
                    alert2.SetConfirmClickListener(new SweetConfirmClick(s =>
                    {
                        homeFragment.GoOffline();
                        availablityStatus = false;
                        TakeDriverOffline();
                        s.Dismiss();
                    }));
                    alert2.Show();
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
                    .WithEndAction(new Runner(() =>
                    {
                        fabToggleOnline.SetImageDrawable(GetDrawable(Resource.Drawable.ic_car_online));
                        fabToggleOnline.Animate()
                            .RotationBy(rotation)
                            .SetDuration(100)
                            .ScaleX(1)
                            .ScaleY(1)
                            .Start();
                    }))
                    .Start();
            };

            bnve = (BottomNavigationView)FindViewById(Resource.Id.bnve);
            BadgeDrawable badge = bnve.GetOrCreateBadge(Resource.Menu.bottomnav);
            badge.SetVisible(true);
            bnve.NavigationItemSelected += Bnve_NavigationItemSelected;

            viewpager = (ViewPager2)FindViewById(Resource.Id.viewpager);
            viewpager.OffscreenPageLimit = 3;
            viewpager.UserInputEnabled = false;
            
        }

        private void Bnve_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
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

        private void SetupViewPager()
        {
            ViewPagerAdapter adapter = new ViewPagerAdapter(SupportFragmentManager, Lifecycle);
            adapter.AddFragment(homeFragment, "Home");
            adapter.AddFragment(earningsFragment, "Earnings");
            adapter.AddFragment(ratingsFragment, "Rating");
            adapter.AddFragment(accountFragment, "Account");
            viewpager.Adapter = adapter;

            homeFragment.CurrentLocation += HomeFragment_CurrentLocation;
            homeFragment.TripActionArrived += HomeFragment_TripActionArrived;
            homeFragment.CallRider += HomeFragment_CallRider;
            homeFragment.Navigate += HomeFragment_Navigate;
            homeFragment.TripActionStartTrip += HomeFragment_TripActionStartTrip;
            homeFragment.TripActionEndTrip += HomeFragment_TripActionEndTrip;
        }

        public async void HomeFragment_TripActionEndTrip(object sender, EventArgs e)
        {
            homeFragment.ResetAfterTrip();
            statusEnum = RideStatusEnum.Normal;

            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            var tripReceipt = await mapHelper.CalculateFares(pickupLatLng, mLastLatLng);
            newTripEventListener.EndTrip(tripReceipt.Fare);
            newTripEventListener = null;
            ShowFareDialog(tripReceipt.Fare, tripReceipt.Distance, tripReceipt.From, tripReceipt.To);
            availablityListener.ReActivate();
        }

        public void HomeFragment_TripActionStartTrip(object sender, EventArgs e)
        {
            var startTripAlert = new SweetAlertDialog(this, SweetAlertDialog.WarningType);
            startTripAlert.SetTitleText("Start Trip");
            startTripAlert.SetContentText("Sure to start trip?");
            startTripAlert.SetCancelText("No");
            startTripAlert.SetConfirmText("Yes");
            startTripAlert.SetConfirmClickListener(new SweetConfirmClick(s=> 
            {
                statusEnum = RideStatusEnum.Ontrip;
                newTripEventListener.UpdateStatus(statusEnum);
                s.Dismiss();
            }));
            startTripAlert.Show();

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
                        //Update and Animate driver movement to pick up location
                        LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
                        mapHelper.UpdateMovement(mLastLatLng, pickupLatLng, ToPositionOf.Rider);

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
                        mapHelper.UpdateMovement(mLastLatLng, destinationLatLng, ToPositionOf.Destination);

                        //Update Location on firebase
                        newTripEventListener.UpdateLocation(mLastLocation);
                        break;
                    }

            }
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
            if (newRideDialog != null)
            {
                newRideDialog.Dismiss();
                newRideDialog = null;
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

       

        private void RideDetailsListener_RideDetailsFound(object sender, RideDetailsListener.RideDetailsEventArgs e)
        {
            if (statusEnum != RideStatusEnum.Normal)
            {
                return;
            }
            newRideDetails = e.RideDetails;

            if (!isBackground)
            {
                CreateNewRequestDialog();
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

        private void CreateNewRequestDialog()
        {
            if(newRideDetails != null)
            {
                newRideDialog = new NewRequestFragment(newRideDetails);
                newRideDialog.Show(SupportFragmentManager, "Request");

                player.Start();

                newRideDialog.OnDismiss(new OnDialogCancel(null, () =>
                {
                    //Stop Alert
                    if (player.IsPlaying && newRideDialog != null)
                    {
                        player.Stop();
                        newRideDialog.Dismiss();
                        newRideDialog = null;

                        //RideRejectDialog rideRejectDialog = new RideRejectDialog();
                        //rideRejectDialog.Show(SupportFragmentManager, "reject ride");

                        availablityListener.ReActivate();
                    }
                }));

                newRideDialog.RideAccepted += async (s2, e2) =>
                {
                    statusEnum = RideStatusEnum.Accepted;
                    newTripEventListener = new NewTripEventListener(newRideDetails.RideId, mLastLocation);
                    newTripEventListener.Create();

                    if (player.IsPlaying && newRideDialog != null)
                    {
                        player.Stop();

                        newRideDialog.Dismiss();
                        newRideDialog = null;
                    }
                     
                    homeFragment.CreateTrip(newRideDetails.RiderName);
                    
                    LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
                    mapHelper.DrawTripOnMap(await mapHelper.GetDirectionJsonAsync(mLastLatLng, pickupLatLng));
                };
            }
            else
            {
                return;
            }
            
        }

        private void AvailablityListener_RideCancelled(object sender, EventArgs e)
        {
            if (newRideDialog != null)
            {
                newRideDialog.Dismiss();
                newRideDialog = null;
                player.Stop();
                player = null;
            }

            Toast.MakeText(this, "New trip was cancelled", ToastLength.Short).Show();
            availablityListener.ReActivate();
        }

        private void ShowFareDialog(double fares, double distance, string from, string to)
        {
            var collectPaymentFragment = new CollectPaymentFragment(fares, distance, from, to);
            collectPaymentFragment.Cancelable = false;
            collectPaymentFragment.Show(SupportFragmentManager, "Collect Payment Fragment");
            collectPaymentFragment.PaymentCollected += (s1, e1) =>
            {
                var earnDataRef = e1.DataRef;
                earnDataRef.Child("totalEarnings").AddListenerForSingleValueEvent(new SingleValueListener(dataSnapshot =>
                {
                    if (!dataSnapshot.Exists())
                        earnDataRef.Child("totalEarnings").SetValueAsync(fares);

                    var earning = dataSnapshot.Value;
                    double totalEarnings = (int)earning + fares;
                    earnDataRef.Child("totalEarnings").SetValueAsync(totalEarnings.ToString());
                    collectPaymentFragment.Dismiss();
                }, dataError =>
                {
                    Toast.MakeText(this, dataError.Message, ToastLength.Short).Show();
                    collectPaymentFragment.Dismiss();
                }));
            };
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
                CreateNewRequestDialog();
                newRideAssigned = false;
            }
        }

        public static void ShowLocationBottomSheet(bool val)
        {
            if (val != true)
            {
                NoLocationBtmSht noLocationBtmSht = new NoLocationBtmSht(_this);
                noLocationBtmSht.Cancelable = false;
                noLocationBtmSht.Show(_this.SupportFragmentManager, "no_location");
            }
            else
            {
                Toast.MakeText(_this, "location on", ToastLength.Long).Show();
            }
        }


        public sealed class OnDialogCancel : Java.Lang.Object, IDialogInterface
        {
            private Action _cancel, _dismiss;
            public OnDialogCancel(Action cancel, Action dismiss)
            {
                _cancel = cancel;
                _dismiss = dismiss;
            }

            public void Cancel()
            {
                _cancel?.Invoke();
            }

            public void Dismiss()
            {
                _dismiss?.Invoke();
            }
        }
    }

    public sealed class Runner : Java.Lang.Object, IRunnable
    {
        private readonly Action _onRun;
        public Runner(Action onRun)
        {
            _onRun = onRun;
        }
        public void Run()
        {
            _onRun?.Invoke();
        }
    }
}