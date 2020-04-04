using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cab360Driver.Helpers;
using static Cab360Driver.Helpers.LocationCallbackHelper;

namespace Cab360Driver.Fragments
{
    public class HomeFragment : Android.Support.V4.App.Fragment, IOnMapReadyCallback
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

        static int UPDATE_INTERVAL = 5; //Seconds
        static int FASTEST_INTERVAL = 5; //Seconds
        static int DISPLACEMENT = 1; //METRES;

        //Layout
        LinearLayout rideInfoLayout;

        //TextView
        TextView riderNameText;

        //Button
        ImageButton cancelTripButton;
        ImageButton callRiderButton;
        ImageButton navigateButton;
        Button tripButton;

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
            SupportMapFragment mapFragment = (SupportMapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map);
            centerMarker = (ImageView)view.FindViewById(Resource.Id.centerMarker);
            mapFragment.GetMapAsync(this);

            cancelTripButton = (ImageButton)view.FindViewById(Resource.Id.cancelTripButton);
            callRiderButton = (ImageButton)view.FindViewById(Resource.Id.callRiderButton);
            navigateButton = (ImageButton)view.FindViewById(Resource.Id.navigateButton);
            tripButton = (Button)view.FindViewById(Resource.Id.tripButton);
            riderNameText = (TextView)view.FindViewById(Resource.Id.riderNameText);
            rideInfoLayout = (LinearLayout)view.FindViewById(Resource.Id.rideInfoLayout);

            tripButton.Click += TripButton_Click;
            callRiderButton.Click += CallRiderButton_Click;
            navigateButton.Click += NavigateButton_Click;

            return view;
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
                tripButton.Text = "Start Trip";
                return;
            }

            if(!tripStarted && driverArrived)
            {
                tripStarted = true;
                TripActionStartTrip.Invoke(this, new EventArgs());
                tripButton.Text = "End Trip";
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
            mainMap = googleMap;
        }

        void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FASTEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
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
            StartLocationUpdates();
        }

        public void GoOffline()
        {
            centerMarker.Visibility = ViewStates.Invisible;
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
            tripButton.Text = "Arrived Pickup";
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