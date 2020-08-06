using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Cab360Driver.Utils;
using Google.Android.Material.FloatingActionButton;
using Java.Lang;
using Refractored.Controls;
using System;
using static Android.Content.IntentSender;
using static Cab360Driver.Helpers.LocationCallbackHelper;

namespace Cab360Driver.Fragments
{
    public class HomeFragment : AndroidX.Fragment.App.Fragment
    {
        private const int RequestCode = 700;
        public EventHandler<OnLocationCaptionEventArgs> CurrentLocation;

        public event EventHandler ShowNotifs;
        public event EventHandler onDestClick;
        public GoogleMap mainMap;

        //Marker
        ImageView centerMarker;
        //Location Client
        LocationRequest mLocationRequest;
        FusedLocationProviderClient locationProviderClient;
        Android.Locations.Location mLastlocation;
        LocationCallbackHelper mLocationCallback = new LocationCallbackHelper();

        //Layout
        RelativeLayout rideInfoLayout;

        //TextView
        TextView riderNameText;

        //Button
        FloatingActionButton cancelTripButton;
        FloatingActionButton callRiderButton;
        FloatingActionButton navigateButton;
        FloatingActionButton ToggleDestBtn;
        Button tripButton;
        public TextSwitcher locationSwitcher;
        private readonly string[] TripBtnTxt = { "Start Trip", "End Trip", "Arrived Pickup" };
        private string[] online = { "Online", "Offline" };

        //Flags
        bool tripCreated = false;
        bool driverArrived = false;
        bool tripStarted = false;

        // Events
        public event EventHandler CallRider;
        public event EventHandler Navigate;
        public event EventHandler TripActionStartTrip;
        public event EventHandler TripActionArrived;
        public event EventHandler TripActionEndTrip;
        private string profileUrl;
        private _BaseCircleImageView profileImg;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CreateLocationRequest();
            if(AppDataHelper.GetCurrentUser() != null)
            {
                var dbRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}");
                dbRef.AddValueEventListener(new SingleValueListener(snapshot =>
                {
                    if (snapshot.Exists())
                    {
                        profileUrl = snapshot.Child("profile_img_url").Value.ToString();
                        Glide.With(Context)
                            .Load(profileUrl)
                            .Into(profileImg)
                            .WaitForLayout();
                    }
                }, error =>
                {

                }));
            }
            else
            {
                return;
            }
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.home, container, false);
            SupportMapFragment mapFragment = ChildFragmentManager.FindFragmentById(Resource.Id.map).JavaCast<SupportMapFragment>();
            mapFragment.GetMapAsync(new OnMapReady(map =>
            {
                mainMap = map;
            }));
            profileImg = (_BaseCircleImageView)view.FindViewById(Resource.Id.home_iv);
            profileImg.Click += ProfileImg_Click;
            return view;
        }

        private void ProfileImg_Click(object sender, EventArgs e)
        {
            ShowNotifs?.Invoke(this, new EventArgs());
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ToggleDestBtn = (FloatingActionButton)view.FindViewById(Resource.Id.togleDestinationFab);
            locationSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.loc_switcher);
            cancelTripButton = (FloatingActionButton)view.FindViewById(Resource.Id.cancelTripButton);
            callRiderButton = (FloatingActionButton)view.FindViewById(Resource.Id.callRiderButton);
            navigateButton = (FloatingActionButton)view.FindViewById(Resource.Id.navigateButton);
            tripButton = (Button)view.FindViewById(Resource.Id.tripButton);
            riderNameText = (TextView)view.FindViewById(Resource.Id.riderNameText);
            rideInfoLayout = (RelativeLayout)view.FindViewById(Resource.Id.linear_home1);
            centerMarker = (ImageView)view.FindViewById(Resource.Id.centerMarker);

            ToggleDestBtn.Click += ToggleDestBtn_Click;
            SetSwitcher();
        }

        private void ToggleDestBtn_Click(object sender, EventArgs e)
        {
            onDestClick?.Invoke(this, new EventArgs());
        }

        private void SetSwitcher()
        {
            int[] animH = { Resource.Animation.slide_in_right, Resource.Animation.slide_out_left };
            int[] animV = { Resource.Animation.slide_in_top, Resource.Animation.slide_out_bottom };

            locationSwitcher.SetFactory(new TextSwitcherUtil(Resource.Style.OnlineTextView, true, Activity));
            locationSwitcher.SetInAnimation(Activity, animH[0]);
            locationSwitcher.SetOutAnimation(Activity, animV[1]);
            locationSwitcher.SetCurrentText(online[1]);
        }

        void NavigateButton_Click(object sender, EventArgs e)
        {
            Navigate?.Invoke(this, new EventArgs());
        }

        void CallRiderButton_Click(object sender, EventArgs e)
        {
            CallRider?.Invoke(this, new EventArgs());
        }

        void TripButton_Click(object sender, EventArgs e)
        {
            if(!driverArrived && tripCreated)
            {
                driverArrived = true;
                TripActionArrived?.Invoke(this, new EventArgs());
                tripButton.Text = TripBtnTxt[0];
                return;
            }

            if(!tripStarted && driverArrived)
            {
                tripStarted = true;
                TripActionStartTrip.Invoke(this, new EventArgs());
                tripButton.Text = TripBtnTxt[1];
                return;
            }

            if (tripStarted)
            {
                TripActionEndTrip?.Invoke(this, new EventArgs());
                return;
            }

        }

        void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(IntegerConstants.GetUpdateInterval());
            mLocationRequest.SetFastestInterval(IntegerConstants.GetFastestInterval());
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(IntegerConstants.GetDisplacement());
            locationProviderClient = LocationServices.GetFusedLocationProviderClient(Activity);
            CheckLocationSettings();
        }

        private void CheckLocationSettings()
        {
            LocationSettingsRequest.Builder builder = new LocationSettingsRequest.Builder()
                .AddLocationRequest(mLocationRequest);
            builder.SetAlwaysShow(true);
            var task = LocationServices.GetSettingsClient(Activity).CheckLocationSettings(builder.Build());
            task.AddOnSuccessListener(new OnSuccessListener(t =>
            {
                mLocationCallback.MyLocation += MLocationCallback_MyLocation;
            })).AddOnFailureListener(new OnFailureListener(e =>
            {
                int statusCode = ((ApiException)e).StatusCode;
                switch (statusCode)
                {
                    case CommonStatusCodes.ResolutionRequired:
                        try
                        {
                            ResolvableApiException resolvable = (ResolvableApiException)e;
                            StartIntentSenderForResult(resolvable.Resolution.IntentSender, RequestCode, null, 0, 0, 0, null);
                        }
                        catch (SendIntentException)
                        {

                        }
                        catch (ClassCastException)
                        {

                        }
                        break;
                    case LocationSettingsStatusCodes.SettingsChangeUnavailable:

                        break;
                }
            }));
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            LocationSettingsStates states = LocationSettingsStates.FromIntent(data);
            switch (requestCode)
            {
                case RequestCode:
                    switch (resultCode)
                    {
                        case (int)Android.App.Result.Ok:
                            mLocationCallback.MyLocation += MLocationCallback_MyLocation;
                            break;

                        case (int)Android.App.Result.Canceled:
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        void MLocationCallback_MyLocation(object sender, OnLocationCaptionEventArgs e)
        {
            mLastlocation = e.Location;
            CurrentLocation?.Invoke(this, new OnLocationCaptionEventArgs { Location = mLastlocation });
            LatLng myposition = new LatLng(mLastlocation.Latitude, mLastlocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15));
            //mainMap.AnimateCamera(CameraUpdateFactory.ZoomIn());
            //CameraPosition cameraPosition = new CameraPosition.Builder()
            //    .Target(myposition)
            //    .Zoom(17)
            //    .Bearing(90)
            //    .Tilt(30)
            //    .Build();
            //mainMap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));

        }

        public void StartLocationUpdates()
        {
            locationProviderClient?.RequestLocationUpdates(mLocationRequest, mLocationCallback, Looper.MainLooper);
        }

        public void StopLocationUpdates()
        {
            locationProviderClient?.RemoveLocationUpdates(mLocationCallback);
        }

        public void GoOnline()
        {
            StartLocationUpdates();
            centerMarker.Visibility = ViewStates.Visible;
            locationSwitcher.SetText(online[0]);
        }

        public void GoOffline()
        {
            StopLocationUpdates();
            locationSwitcher.SetText(online[1]);
            centerMarker.Visibility = ViewStates.Invisible;
        }

        public void CreateTrip(string ridername)
        {
            centerMarker.Visibility = ViewStates.Invisible;
            riderNameText.Text = ridername;
            rideInfoLayout.Visibility = ViewStates.Visible;
            tripCreated = true;
        }

        public void ResetAfterTrip()
        {
            tripButton.Text = TripBtnTxt[3];
            centerMarker.Visibility = ViewStates.Visible;
            riderNameText.Text = "";
            rideInfoLayout.Visibility = ViewStates.Invisible;
            tripCreated = false;
            driverArrived = false;
            tripStarted = false;
            mainMap.Clear();
            mainMap.TrafficEnabled = false;
            mainMap.UiSettings.ZoomControlsEnabled = false;
        }

        internal sealed class OnMapReady : Java.Lang.Object, IOnMapReadyCallback
        {
            private Action<GoogleMap> _onMapReady;
            public OnMapReady(Action<GoogleMap> onMapReady)
            {
                _onMapReady = onMapReady;
            }
            void IOnMapReadyCallback.OnMapReady(GoogleMap googleMap)
            {
                _onMapReady?.Invoke(googleMap);
            }
        }
    }
}