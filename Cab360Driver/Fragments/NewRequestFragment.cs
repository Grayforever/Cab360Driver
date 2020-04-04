using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace Cab360Driver.Fragments
{
    public class NewRequestFragment : Android.Support.V4.App.DialogFragment
    {

        //Views
        private Button acceptRideButton;
        private Button rejectRideButton;
        private TextView pickupAddressText;
        private TextView destinationAddressText;

        string mPickupAddress;
        string mDestinationAddress;

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
            var view =   inflater.Inflate(Resource.Layout.newrequest_dialogue, container, false);

            GetControls(view);

            return view;
        }

        private void GetControls(View view)
        {
            pickupAddressText = (TextView)view.FindViewById(Resource.Id.newridePickupText);
            destinationAddressText = (TextView)view.FindViewById(Resource.Id.newrideDestinationText);

            pickupAddressText.Text = mPickupAddress;
            destinationAddressText.Text = mDestinationAddress;

            acceptRideButton = (Button)view.FindViewById(Resource.Id.ride_accept_btn);
            rejectRideButton = (Button)view.FindViewById(Resource.Id.ride_decline_btn);

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
