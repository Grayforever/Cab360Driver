using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Google.Android.Material.FloatingActionButton;
using System;
using static Android.Widget.ViewSwitcher;
using static Cab360Driver.Helpers.LocationCallbackHelper;

namespace Cab360Driver.Fragments
{
    public class HomeFragment : AndroidX.Fragment.App.Fragment, IOnMapReadyCallback, IViewFactory
    {
        public EventHandler<OnLocationCaptionEventArgs> CurrentLocation;
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
        Button tripButton;

        private TextSwitcher StatusSwitcher;
        private readonly string[] status = {"OFFLINE", "ONLINE"};
        private readonly string[] TripBtnTxt = { "Start Trip", "End Trip", "Arrived Pickup" };

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



        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CreateLocationRequest();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.home, container, false);
            SupportMapFragment mapFragment = ChildFragmentManager.FindFragmentById(Resource.Id.map).JavaCast<SupportMapFragment>();
            centerMarker = (ImageView)view.FindViewById(Resource.Id.centerMarker);
            mapFragment.GetMapAsync(this);

            StatusSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_temperature);
            InitSwitcher();

            cancelTripButton = (FloatingActionButton)view.FindViewById(Resource.Id.cancelTripButton);
            callRiderButton = (FloatingActionButton)view.FindViewById(Resource.Id.callRiderButton);
            navigateButton = (FloatingActionButton)view.FindViewById(Resource.Id.navigateButton);
            tripButton = (Button)view.FindViewById(Resource.Id.tripButton);
            riderNameText = (TextView)view.FindViewById(Resource.Id.riderNameText);
            rideInfoLayout = (RelativeLayout)view.FindViewById(Resource.Id.linear_home1);

            tripButton.Click += TripButton_Click;
            callRiderButton.Click += CallRiderButton_Click;
            navigateButton.Click += NavigateButton_Click;

            return view;
        }

        void InitSwitcher()
        {

            StatusSwitcher.SetFactory(this);
            StatusSwitcher.SetInAnimation(Activity, Resource.Animation.slide_in_right);
            StatusSwitcher.SetOutAnimation(Activity, Resource.Animation.slide_out_left);
            StatusSwitcher.SetCurrentText(status[0]);
        }

        public View MakeView()
        {
            var textOnline = new TextView(Activity);
            textOnline.Gravity = GravityFlags.Center| GravityFlags.CenterVertical;
            textOnline.SetTextAppearance(Resource.Style.TextAppearance_AppCompat_Subhead);
            return textOnline;
        }

        void NavigateButton_Click(object sender, EventArgs e)
        {
            Navigate.Invoke(this, new EventArgs());
        }

        void CallRiderButton_Click(object sender, EventArgs e)
        {
            CallRider.Invoke(this, new EventArgs());
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
                TripActionEndTrip.Invoke(this, new EventArgs());
                return;
            }

        }


        public void OnMapReady(GoogleMap googleMap)
        {  
            try
            {
                bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(Activity, Resource.Raw.gray_mapstyle));

                if (!success)
                {
                    Toast.MakeText(Activity, "style passing failed", ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Toast.MakeText(Activity, e.Message, ToastLength.Short).Show();
            }
            mainMap = googleMap;

        }

        void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(IntegerConstants.GetUpdateInterval());
            mLocationRequest.SetFastestInterval(IntegerConstants.GetFastestInterval());
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(IntegerConstants.GetDisplacement());
            mLocationCallback.MyLocation += MLocationCallback_MyLocation;
            locationProviderClient = LocationServices.GetFusedLocationProviderClient(Activity);
        }

        void MLocationCallback_MyLocation(object sender, OnLocationCaptionEventArgs e)
        {
            mLastlocation = e.Location;

            //Update our Lastlocation on the Map
            LatLng myposition = new LatLng(mLastlocation.Latitude, mLastlocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 15));

            //Sends Location to Mainactivity
            CurrentLocation?.Invoke(this, new OnLocationCaptionEventArgs { Location = e.Location });

        }


        void StartLocationUpdates()
        {
            locationProviderClient.RequestLocationUpdates(mLocationRequest, mLocationCallback, null);
        }

        void StopLocationUpdates()
        {
            locationProviderClient.RemoveLocationUpdates(mLocationCallback);
        }

        public void GoOnline()
        {
            centerMarker.Visibility = ViewStates.Visible;
            StatusSwitcher.SetText(status[1]);
            StartLocationUpdates();
        }

        public void GoOffline()
        {
            centerMarker.Visibility = ViewStates.Invisible;
            StatusSwitcher.SetText(status[0]);
            StopLocationUpdates();
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
    }
}