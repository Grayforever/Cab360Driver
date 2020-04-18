using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.FloatingActionButton;
using System;

namespace Cab360Driver.Fragments
{
    public class NewRequestFragment : AndroidX.Fragment.App.DialogFragment
    {

        //Views
        private MaterialButton acceptRideButton;
        private FloatingActionButton rejectRideButton;

        string mPickupAddress;
        string mDestinationAddress;
        string mDuration;
        string mDistance;

        //Events
        public event EventHandler RideAccepted;
        public event EventHandler RideRejected;

        public NewRequestFragment(string pickupAddress, string destinationAddress)
        {
            mPickupAddress = pickupAddress;
            mDestinationAddress = destinationAddress;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view =   inflater.Inflate(Resource.Layout.new_ride_request_overlay, container, false);

            GetControls(view);

            return view;
        }

        private void GetControls(View view)
        {
            acceptRideButton = (MaterialButton)view.FindViewById(Resource.Id.accept_ride_btn);
            rejectRideButton = (FloatingActionButton)view.FindViewById(Resource.Id.decline_ride_fab);

            acceptRideButton.Click += AcceptRideButton_Click;
            rejectRideButton.Click += RejectRideButton_Click;
        }

        void AcceptRideButton_Click(object sender, EventArgs e)
        {
            RideAccepted?.Invoke(this, new EventArgs());
        }

        void RejectRideButton_Click(object sender, EventArgs e)
        {
            RideRejected?.Invoke(this, new EventArgs());
        }

    }
}
