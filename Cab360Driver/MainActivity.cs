using Android;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.DataModels;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Fragments;
using Cab360Driver.Helpers;
using Cab360Driver.Utils;
using CN.Pedant.SweetAlert;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomSheet;
using Google.Places;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cab360Driver
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize| Android.Content.PM.ConfigChanges.SmallestScreenSize| Android.Content.PM.ConfigChanges.ScreenLayout| Android.Content.PM.ConfigChanges.Orientation, 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
        WindowSoftInputMode = SoftInput.AdjustResize, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]

    public class MainActivity : AppCompatActivity
    {
        private HomeFragment homeFragment = new HomeFragment();
        private RatingsFragment ratingsFragment = new RatingsFragment();
        private EarningsFragment earningsFragment = new EarningsFragment();
        private AccountFragment accountFragment = new AccountFragment();

        private NewRequestFragment newRideDialog;
        private readonly NotifsRecyclerAdapter adapter;
        private RecyclerView notifsRecycler;

        private BottomNavigationView bottomNav;
        private Bitvale.SwitcherLib.SwitcherC onlineSwitcher;

        AvailablityListener availablityListener;
        RideDetailsListener rideDetailsListener;
        NewTripEventListener newTripEventListener;
        Android.Locations.Location mLastLocation;
        LatLng mLastLatLng;

        //Flags
        private bool availablityStatus;
        private bool isBackground;
        private bool newRideAssigned;
        private RideStatusEnum statusEnum = RideStatusEnum.Normal;
        private const int RequestID = 0;
        private const int containerId = Resource.Id.container;

        //Datamodels
        private RideDetails newRideDetails;

        //MediaPlayer
        private MediaPlayer player;

        //Helpers
        private MapFunctionHelper mapHelper;
        private AndroidX.Fragment.App.Fragment activeFragment;
        private AndroidX.Fragment.App.FragmentManager fragmentManager;
        private BottomSheetBehavior NotifBehavior;
        private BottomSheetBehavior QrBehavior;
        private BottomSheetBehavior defaultBehavior;
        static SwipeControllerUtils sc;
        private const int REQUEST_CODE_PLACE = 99;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            MapsInitializer.Initialize(this);
            onlineSwitcher = FindViewById<Bitvale.SwitcherLib.SwitcherC>(Resource.Id.switcher);
            bottomNav = (BottomNavigationView)FindViewById(Resource.Id.bnve);
            LoadFragments();
            InitBottomSheets();
            
            bottomNav.NavigationItemSelected += Bnve_NavigationItemSelected;
            onlineSwitcher.SetOnCheckedChangeListener(@checked =>
            {
                switch (@checked)
                {
                    case true:
                        if (!CheckSpecialPermission())
                        {
                            return;
                        }
                        else
                        {
                            availablityStatus = true;
                            homeFragment.GoOnline();
                        }
                        break;
                    default:
                        if (availablityStatus)
                        {
                            homeFragment.GoOffline();
                            availablityStatus = false;
                            TakeDriverOffline();
                        }

                        break;
                }
            });
        }

        private void LoadFragments()
        {
            activeFragment = homeFragment;
            fragmentManager = SupportFragmentManager;
            fragmentManager.BeginTransaction()
                           .Add(containerId, homeFragment, "Home")
                           .Commit();

            fragmentManager.BeginTransaction()
                           .Add(containerId, earningsFragment, "Earnings")
                           .Hide(earningsFragment)
                           .Commit();

            fragmentManager.BeginTransaction()
                           .Add(containerId, ratingsFragment, "Ratings")
                           .Hide(ratingsFragment)
                           .Commit();

            fragmentManager.BeginTransaction()
                           .Add(containerId, accountFragment, "Account")
                           .Hide(accountFragment)
                           .Commit();
        }

        private void InitBottomSheets()
        {
            var notifRoot = (ConstraintLayout)FindViewById(Resource.Id.notifs_root);
            var qrRoot = (ConstraintLayout)FindViewById(Resource.Id.qr_root);
            var closeBottomBtn = FindViewById<TextView>(Resource.Id.notifs_hdr);
            var qrImageView = FindViewById<ImageView>(Resource.Id.qr_iv);
            qrImageView.PostDelayed(async ()=> 
                {
                    qrImageView.SetImageBitmap(await Task.Run(() => new QrGenUtil().TextToImageEncode(AppDataHelper.GetCurrentUser().Uid)));
                }, 1000);
            NotifBehavior = BottomSheetBehavior.From(notifRoot);
            NotifBehavior.Hideable = true;
            NotifBehavior.State = BottomSheetBehavior.StateHidden;
            NotifBehavior.AddBottomSheetCallback(new BottomSheetCallback());

            closeBottomBtn.Click += delegate
            {
                NotifBehavior.State = BottomSheetBehavior.StateHidden;
            };

            QrBehavior = BottomSheetBehavior.From(qrRoot);
            QrBehavior.Hideable = true;
            QrBehavior.State = BottomSheetBehavior.StateHidden;
            QrBehavior.AddBottomSheetCallback(new BottomSheetCallback());
        }

        private void Bnve_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.action_earning:
                    fragmentManager.BeginTransaction()
                        .Hide(activeFragment)
                        .Show(earningsFragment)
                        .Commit();
                    activeFragment = earningsFragment;
                    break;

                case Resource.Id.action_home:
                    fragmentManager.BeginTransaction()
                        .Hide(activeFragment)
                        .Show(homeFragment)
                        .Commit();
                    activeFragment = homeFragment;

                    break;

                case Resource.Id.action_rating:
                    fragmentManager.BeginTransaction()
                        .Hide(activeFragment)
                        .Show(ratingsFragment)
                        .Commit();
                    activeFragment = ratingsFragment;
                    break;

                case Resource.Id.action_account:
                    fragmentManager.BeginTransaction()
                        .Hide(activeFragment)
                        .Show(accountFragment)
                        .Commit();
                    activeFragment = accountFragment;
                    break;

                default:
                    return;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            player = MediaPlayer.Create(this, Resource.Raw.alert);
            mapHelper = new MapFunctionHelper(homeFragment.mainMap);
            CheckSpecialPermission();
            InitListeners();
            SetUpAdapter();
            InitPlacesApi();
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

        private void InitListeners()
        {
            accountFragment.onQrClick += AccountFragment_onQrClick;
            homeFragment.ShowNotifs += HomeFragment_ShowNotifs;
            homeFragment.onDestClick += HomeFragment_onDestClick;
            homeFragment.CurrentLocation += HomeFragment_CurrentLocation;
            homeFragment.TripActionArrived += HomeFragment_TripActionArrived;
            homeFragment.CallRider += HomeFragment_CallRider;
            homeFragment.Navigate += HomeFragment_Navigate;
            homeFragment.TripActionStartTrip += HomeFragment_TripActionStartTrip;
            homeFragment.TripActionEndTrip += HomeFragment_TripActionEndTrip;
        }

        private void AccountFragment_onQrClick(object sender, EventArgs e)
        {
            defaultBehavior = QrBehavior;
            BottomSheetToggle();
        }

        private void SetUpAdapter()
        {
            notifsRecycler = FindViewById<RecyclerView>(Resource.Id.nitifs_recycler);
            List<NotifsDataModel> notifsList = new List<NotifsDataModel>()
            {
                new NotifsDataModel(){Title = "New ride request", Image = Resource.Drawable.tips, DateTime = DateTime.UtcNow},
                new NotifsDataModel(){Title = "Maintenance section is due", Image = Resource.Drawable.school_bell, DateTime = DateTime.UtcNow},
            };

            NotifsRecyclerAdapter adapter = new NotifsRecyclerAdapter(notifsList);
            notifsRecycler.SetLayoutManager(new LinearLayoutManager(this));
            notifsRecycler.SetAdapter(adapter);
            SwipeActions sa = new SwipeActions((pos1 => { }),
            (pos2 =>
            {
                adapter._notificationsList.RemoveAt(pos2);
                adapter.NotifyItemRemoved(pos2);
                adapter.NotifyItemRangeChanged(pos2, adapter.ItemCount);
            }));
            sc = new SwipeControllerUtils(sa);
            ItemTouchHelper helper = new ItemTouchHelper(sc);
            helper.AttachToRecyclerView(notifsRecycler);
            notifsRecycler.AddItemDecoration(new ItemDecorator(sc));
        }

        private void InitPlacesApi()
        {
            if (PlacesApi.IsInitialized)
            {
                return;
            }
            PlacesApi.Initialize(this, GetString(Resource.String.google_api_key));
        }

        public async void HomeFragment_TripActionEndTrip(object sender, EventArgs e)
        {
            homeFragment.ResetAfterTrip();
            statusEnum = RideStatusEnum.Normal;

            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            var fare = await mapHelper.CalculateFares(pickupLatLng, mLastLatLng);
            newTripEventListener.EndTrip(fare);
            newTripEventListener = null;
            ShowFareDialog(fare);
            availablityListener.ReActivate();
        }

        public void HomeFragment_TripActionStartTrip(object sender, EventArgs e)
        {
            var startTripAlert = new SweetAlertDialog(this, SweetAlertDialog.WarningType);
            startTripAlert.SetTitleText("Start Trip");
            startTripAlert.SetContentText("Sure to start trip?");
            startTripAlert.SetCancelText("No");
            startTripAlert.SetConfirmText("Yes");
            startTripAlert.SetConfirmClickListener(new SweetConfirmClick(s =>
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

                case RideStatusEnum.Cancelled:
                    break;
                case RideStatusEnum.Normal:
                    break;
                case RideStatusEnum.Ended:
                    break;
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
            catch (Exception exception)
            {
                Toast.MakeText(this, exception.Message, ToastLength.Short).Show();
            }
        }

        private void HomeFragment_ShowNotifs(object sender, EventArgs e)
        {
            defaultBehavior = NotifBehavior;
            BottomSheetToggle();
        }

        private void BottomSheetToggle()
        {
            if (defaultBehavior != null)
            {
                switch (defaultBehavior.State)
                {
                    case BottomSheetBehavior.StateHidden:
                        defaultBehavior.State = BottomSheetBehavior.StateExpanded;
                        break;
                    case BottomSheetBehavior.StateExpanded:
                        defaultBehavior.State = BottomSheetBehavior.StateHidden;
                        break;
                }
            }
            else
            {
                return;
            }
        }

        private void HomeFragment_onDestClick(object sender, EventArgs e)
        {
            StartAutoComplete();
        }

        private void StartAutoComplete()
        {
            List<Place.Field> fields = new List<Place.Field>
            {
                Place.Field.Id,
                Place.Field.Name,
                Place.Field.LatLng,
                Place.Field.Address
            };

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
                .SetCountry("GH")
                .Build(this);

            StartActivityForResult(intent, REQUEST_CODE_PLACE);
        }

        protected async override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case REQUEST_CODE_PLACE:
                    switch (resultCode)
                    {
                        case Android.App.Result.Ok:
                            {
                                var place = Autocomplete.GetPlaceFromIntent(data);
                                switch (availablityStatus)
                                {
                                    case true:
                                        mapHelper.DrawTripOnMap(await mapHelper.GetDirectionJsonAsync(mLastLatLng, place.LatLng));
                                        homeFragment.locationSwitcher.SetText(place.Name);
                                        break;
                                    default:
                                        Toast.MakeText(this, "Please go online first", ToastLength.Short).Show();
                                        break;
                                }
                                break;
                            }

                        case Android.App.Result.Canceled:
                            break;
                        case Android.App.Result.FirstUser:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void CreateNewRequestDialog()
        {
            if (newRideDetails != null)
            {
                newRideDialog = new NewRequestFragment(newRideDetails);
                newRideDialog.Show(SupportFragmentManager, "Request");

                player.Start();

                newRideDialog.OnDismiss(new OnDialogCancel(null, () =>
                {
                    if (player.IsPlaying && newRideDialog != null)
                    {
                        player.Stop();
                        newRideDialog.DismissAllowingStateLoss();
                        newRideDialog = null;

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

                        newRideDialog.DismissAllowingStateLoss();
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

        private void OpenGoogleMap(string uriString)
        {
            Android.Net.Uri googleMapIntentUri = Android.Net.Uri.Parse(uriString);
            Intent mapIntent = new Intent(Intent.ActionView, googleMapIntentUri);
            mapIntent.SetPackage("com.google.android.apps.maps");
            StartActivity(mapIntent);
        }

        private string NavUriString(double lat, double lng) => $"{StringConstants.GetNavigateBaseGateway()}{lat},{lng}";

        private void TakeDriverOnline()
        {
            if (mLastLocation == null)
            {
                return;
            }
            else
            {
                availablityListener = new AvailablityListener(this);
                availablityListener.Create(mLastLocation);
                availablityListener.RideAssigned += AvailablityListener_RideAssigned;
                availablityListener.RideTimedOut += AvailablityListener_RideTimedOut;
                availablityListener.RideCancelled += AvailablityListener_RideCancelled;
            }
        }

        private void TakeDriverOffline()
        {
            if(availablityListener == null)
            {
                return;
            }

            availablityListener.RemoveListener();
            availablityListener = null;

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
            if (string.IsNullOrEmpty(e.RideId))
            {
                return;
            }
            else
            {
                //Get Details
                rideDetailsListener = new RideDetailsListener();
                rideDetailsListener.Create(e.RideId);
                rideDetailsListener.RideDetailsFound += RideDetailsListener_RideDetailsFound;
                rideDetailsListener.RideDetailsNotFound += RideDetailsListener_RideDetailsNotFound;
            }
        }

        private void RideDetailsListener_RideDetailsNotFound(object sender, EventArgs e)
        {

        }

        private void RideDetailsListener_RideDetailsFound(object sender, RideDetailsListener.RideDetailsEventArgs e)
        {
            newRideDetails = e.RideDetails;
            if (statusEnum != RideStatusEnum.Normal)
            {
                return;
            }

            if (isBackground)
            {
                newRideAssigned = true;
                if ((int)Build.VERSION.SdkInt >= 26)
                {
                    NotificationHelper notificationHelper = new NotificationHelper();
                    notificationHelper.NotifyVersion26(this, Resources, (NotificationManager)GetSystemService(NotificationService));
                }
            }
            else
            {
                CreateNewRequestDialog();
            }
        }    

        private void AvailablityListener_RideCancelled(object sender, EventArgs e)
        {
            if (newRideDialog != null)
            {
                newRideDialog.DismissAllowingStateLoss();
                newRideDialog = null;
                player.Stop();
                player = null;
            }

            Toast.MakeText(this, "New trip was cancelled", ToastLength.Short).Show();
            availablityListener.ReActivate();
        }

        private void ShowFareDialog(double fare)
        {
            var collectPaymentFragment = new CollectPaymentFragment(newRideDetails, fare);
            collectPaymentFragment.Cancelable = false;
            AndroidX.Fragment.App.FragmentTransaction ft = SupportFragmentManager.BeginTransaction();
            ft.Add(collectPaymentFragment, "collect_pay_frag");
            ft.CommitAllowingStateLoss();
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
            if (!newRideAssigned)
            {
                return;
            }
            CreateNewRequestDialog();
            newRideAssigned = false;
        }

        public override void OnBackPressed()
        {
            switch (defaultBehavior.State)
            {
                case BottomSheetBehavior.StateExpanded:
                case BottomSheetBehavior.StateHalfExpanded:
                    defaultBehavior.State = BottomSheetBehavior.StateHidden;
                    break;
                default:
                    base.OnBackPressed();
                    break;
            }
        } 
    }
}