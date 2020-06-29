using Android.OS;
using Android.Views;
using Android.Widget;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Java.Util;
using System;

namespace Cab360Driver.Fragments
{
    public class CollectPaymentFragment : AndroidX.Fragment.App.DialogFragment
    {
        private readonly double _fares;
        private readonly double _distance;
        private readonly string _from;
        private readonly string _to;
        private FirebaseUser currUser;

        public event EventHandler<PaymentCollectedEventArgs> PaymentCollected;
        public class PaymentCollectedEventArgs : EventArgs
        {
            public DatabaseReference DataRef { get; set; }
        }

        public CollectPaymentFragment(double fares, double distance, string from, string to)
        {
            _fares = fares;
            _distance = distance;
            _from = from;
            _to = to;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
            currUser = AppDataHelper.GetCurrentUser();
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.collect_payment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            GetControls(view);
        }

        private void GetControls(View view)
        {
            var totalfaresText = (TextView)view.FindViewById(Resource.Id.totalfaresText);
            var totalDistance = (TextView)view.FindViewById(Resource.Id.totalDistanceText);
            var fromText = (TextView)view.FindViewById(Resource.Id.fare_frm_txt);
            var toText = (TextView)view.FindViewById(Resource.Id.fare_to_txt);
            var collectPayButton = (Button)view.FindViewById(Resource.Id.collectPayButton);

            totalfaresText.Text = $"¢{_fares}";
            totalDistance.Text = $"{_distance}km";
            fromText.Text = _from;
            toText.Text = _to;

            collectPayButton.Click += (s1, e1) =>
            {
                var dataObject = HashEarnings();
                var mEarningsRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{currUser.Uid}/MyEarnings/{DateTime.UtcNow}");
                mEarningsRef.SetValue(dataObject)
                    .AddOnSuccessListener(new OnSuccessListener(result =>
                    {
                        PaymentCollected?.Invoke(this, new PaymentCollectedEventArgs { DataRef = mEarningsRef });
                    }))
                    .AddOnFailureListener(new OnFailureListener(e =>
                    {
                        Toast.MakeText(Activity, e.Message, ToastLength.Short).Show();
                    }));
            };
        }

        private HashMap HashEarnings()
        {
            HashMap earnMap = new HashMap();
            earnMap.Put("totalDistance", _distance);
            earnMap.Put("rideFare", _fares);
            earnMap.Put("from", _from);
            earnMap.Put("to", _to);
            return earnMap;
        }

    }
}
