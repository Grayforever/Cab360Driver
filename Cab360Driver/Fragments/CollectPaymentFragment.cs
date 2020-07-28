using Android.OS;
using Android.Views;
using Android.Widget;
using Cab360Driver.DataModels;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase;
using Firebase.Database;
using Java.Util;
using System;

namespace Cab360Driver.Fragments
{
    public class CollectPaymentFragment : AndroidX.Fragment.App.DialogFragment
    {
        private RideDetails _rideDetails;
        private double _fare;
        public CollectPaymentFragment(RideDetails rideDetails, double fare)
        {
            _rideDetails = rideDetails;
            _fare = fare;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
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

            totalfaresText.Text = "Gh¢" + _fare;
            totalDistance.Text = _rideDetails.Distance + "km";
            fromText.Text = _rideDetails.PickupAddress;
            toText.Text = _rideDetails.DestinationAddress;

            collectPayButton.Click += (s1, e1) =>
            {
                var earnRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}/earnings");
                earnRef.AddValueEventListener(new SingleValueListener(
                    snapshot =>
                    {
                        if (!snapshot.Exists())
                        {
                            return;
                        }

                        double exist_earn = double.Parse(snapshot.Child("tot_earnings").Value.ToString());
                        double totEarn = _fare + exist_earn;

                        HashMap earnInfo = new HashMap();
                        earnInfo.Put("date", DateTime.UtcNow.Date.ToString());
                        earnInfo.Put("fare", _fare.ToString());
                        earnInfo.Put("from", _rideDetails.PickupAddress);
                        earnInfo.Put("to", _rideDetails.DestinationAddress);
                        earnInfo.Put("rider_phone", _rideDetails.RiderPhone);

                        earnRef.Child("rides").Child(_rideDetails.RideId).SetValue(earnInfo).AddOnCompleteListener(new OnCompleteListener(
                        async t =>
                        {
                            try
                            {
                                if (t.IsSuccessful)
                                {
                                    await earnRef.Child("tot_earnings").SetValueAsync(totEarn.ToString());
                                }
                                else
                                {
                                    throw t.Exception;
                                    
                                }
                            }
                            catch (DatabaseException de)
                            {
                                Toast.MakeText(Context, de.Message, ToastLength.Short).Show();
                            }
                            catch (FirebaseNetworkException fne)
                            {
                                Toast.MakeText(Context, fne.Message, ToastLength.Short).Show();
                            }
                            catch (Exception e)
                            {
                                Toast.MakeText(Context, e.Message, ToastLength.Short).Show();
                            }


                        }));
                    },
                    error =>
                    {
                        Toast.MakeText(Activity, error.Message, ToastLength.Short).Show();
                    }));
            };
        }
    }
}
