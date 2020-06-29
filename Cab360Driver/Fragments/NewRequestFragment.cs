using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
using Cab360Driver.DataModels;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Button;
using System;

namespace Cab360Driver.Fragments
{
    public class NewRequestFragment : BottomSheetDialogFragment
    {
        private MaterialButton acceptRideButton;
        private RideDetails _rideDetails;
        public event EventHandler RideAccepted;

        public NewRequestFragment([Nullable]RideDetails rideDetails)
        {
            _rideDetails = rideDetails;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_ModalDialog);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.new_ride_request_overlay, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var fromTxt = view.FindViewById<TextView>(Resource.Id.new_ride_frm_tv);
            var toTxt = view.FindViewById<TextView>(Resource.Id.new_ride_to_tv);
            var distanceTxt = view.FindViewById<TextView>(Resource.Id.box1_tv);
            var durationTxt = view.FindViewById<TextView>(Resource.Id.box2_tv);
            var fareTxt = view.FindViewById<TextView>(Resource.Id.box3_tv);

            acceptRideButton = (MaterialButton)view.FindViewById(Resource.Id.new_ride_acceptBtn);
            if (_rideDetails != null)
            {
                fromTxt.Text = _rideDetails.PickupAddress;
                toTxt.Text = _rideDetails.DestinationAddress;
                distanceTxt.Text = _rideDetails.Distance;
                durationTxt.Text = _rideDetails.Duration;
                fareTxt.Text = _rideDetails.Fare;
            }
            else
            {
                return;
            }

            acceptRideButton.Click += (s1, e1) =>
            {
                RideAccepted?.Invoke(this, new EventArgs());
            };
        }
    }
}
