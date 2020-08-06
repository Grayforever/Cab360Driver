using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using Cab360Driver.Adapters;
using Cab360Driver.DataModels;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Database;
using Google.Android.Material.Button;
using Microcharts;
using Microcharts.Droid;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cab360Driver.Fragments
{
    public class EarningsFragment : Fragment
    {
        private string[] chartArray = { "Radar Chart", "Point Chart", "Line Chart" };
        private List<EarnModel> earningList = new List<EarnModel>();
        private ChartView ChartView;
        private TextView totalTripTxt;
        private TextView totBalanceTxt;
        private string tot_earnings;
        private float textSize => Context.Resources.GetDimension(Resource.Dimension.abc_text_size_body_1_material);

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            
        }

        private FragmentTransaction ShowDetails()
        {
            FragmentTransaction ft = ChildFragmentManager.BeginTransaction();
            WithdrawFragment earnDetFragment = new WithdrawFragment();
            ft.Add(earnDetFragment, "earning_details");
            return ft;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.earnings, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ChartView = view.FindViewById<ChartView>(Resource.Id.earn_chartview);
            var ChartTypespinner = view.FindViewById<AppCompatSpinner>(Resource.Id.chart_type_spinner);
            var adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, chartArray);
            var withdrawBtn = view.FindViewById<MaterialButton>(Resource.Id.earn_withdraw_btn);
            totalTripTxt = view.FindViewById<TextView>(Resource.Id.earn_tot_trip_txt);
            totBalanceTxt = view.FindViewById<TextView>(Resource.Id.earn_tv2);
            var totalKmTxt = view.FindViewById<TextView>(Resource.Id.earn_tot_dis_txt);
            var earnTotAmtTxt = view.FindViewById<TextView>(Resource.Id.earn_amt_hd);
            var earnAmtTxt = view.FindViewById<TextView>(Resource.Id.earn_trip_txt);
            var earnTrip = view.FindViewById<TextView>(Resource.Id.earn_trip_txt);

            GetDbAsync();
            withdrawBtn.Click += WithdrawBtn_Click;
            ChartTypespinner.Adapter = adapter;
            ChartTypespinner.ItemSelected += ChartTypespinner_ItemSelected;
            
        }

        private async void GetDbAsync()
        {
            await Task.Run(() =>
            {
                var earnRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}/earnings");
                earnRef.AddValueEventListener(new SingleValueListener(
                    snapshot =>
                    {
                        if (snapshot.Exists())
                        {
                            var rides = snapshot.Child("rides").Children.ToEnumerable<DataSnapshot>();
                            tot_earnings = snapshot.Child("tot_earnings").Value.ToString();

                            earningList.Clear();
                            earningList.AddRange(from ride in rides
                                                 let earnModel = new EarnModel
                                                 {
                                                     ID = ride.Key,
                                                     Fare = ride.Child("fare").Value.ToString(),
                                                     Date = ride.Child("date").Value.ToString()
                                                 }
                                                 select earnModel);
                            DrawChart(0);

                            totBalanceTxt.Text = $"Gh¢{tot_earnings}.00";
                            totalTripTxt.Text = earningList.Count.ToString();
                        }
                        else
                        {
                            earnRef.Child("tot_earnings").SetValue("0");
                        }
                    },
                    error =>
                    {
                        Toast.MakeText(Activity, error.Message, ToastLength.Short).Show();
                    }));
            });

        }

        private void WithdrawBtn_Click(object sender, EventArgs e)
        {
            FragmentTransaction ft = ShowDetails();
            ft.CommitAllowingStateLoss();
        }

        private void ChartTypespinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            DrawChart(e.Position);
        }

        private void DrawChart(int chartIndex)
        {
            if(earningList.Count == 0)
            {
                return;
            }

            List<ChartEntry> DataEntry = new List<ChartEntry>();
            for (int i = Math.Max(earningList.Count - 5, 0); i < earningList.Count; ++i)
            {
                DataEntry.Add(new ChartEntry(float.Parse(earningList[i].Fare))
                {
                    Label = $"Ride {i + 1}",
                    ValueLabel = earningList[i].Fare,
                    Color = SKColor.Parse(HexConverter())
                });
            }

            switch (chartIndex)
            {
                case 0:
                    var chartRader = new RadarChart()
                    {
                        Entries = DataEntry,
                        IsAnimated = true
                    };

                    ChartView.Chart = chartRader;
                    break;

                case 1:
                    var chartPoint = new PointChart()
                    {
                        Entries = DataEntry,
                        IsAnimated = true
                    };

                    ChartView.Chart = chartPoint;
                    break;

                case 2:
                    var chartLine = new LineChart()
                    {
                        Entries = DataEntry,
                        IsAnimated = true
                    };

                    ChartView.Chart = chartLine;
                    break;

                default:
                    var chart = new RadarChart()
                    {
                        Entries = DataEntry,
                        IsAnimated = true
                    };

                    ChartView.Chart = chart;
                    break;
            }
        }

        private static string HexConverter()
        {
            Android.Graphics.Color c = new Android.Graphics.Color((int)(Java.Lang.Math.Random() * 0x1000000));
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}