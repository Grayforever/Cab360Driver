using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.Helpers;
using Google.Android.Material.Button;
using Microcharts;
using Microcharts.Droid;
using SkiaSharp;
using System.Collections.Generic;

namespace Cab360Driver.Fragments
{
    public class EarningsFragment : AndroidX.Fragment.App.Fragment
    {
        private string[] chartArray = { "Bar Chart", "Radar Chart", "Point Chart", "Radial Chart", "Line Chart" };
        private string[] days = { "Monday", "Tuesday", "Wedneday", "Thursday", "Friday", "Saturday", "Sunday" };
        private ChartView ChartView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.earnings, container, false);
            InitControls(view);
            DrawChart(0);
            return view;
        }

        private void InitControls(View view)
        {
            ChartView = view.FindViewById<ChartView>(Resource.Id.earn_chartview);

            var ChartTypespinner = view.FindViewById<AppCompatSpinner>(Resource.Id.chart_type_spinner);

            var adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, chartArray);

            var withdrawBtn = view.FindViewById<MaterialButton>(Resource.Id.earn_withdraw_btn);

            var totalTripTxt = view.FindViewById<TextView>(Resource.Id.earn_tot_trip_txt);

            var totBalanceTxt = view.FindViewById<TextView>(Resource.Id.earn_tv2);
            totBalanceTxt.Text = AppDataHelper.GetTotEarnings();

            var totalKmTxt = view.FindViewById<TextView>(Resource.Id.earn_tot_dis_txt);

            var earnTotAmtTxt = view.FindViewById<TextView>(Resource.Id.earn_amt_hd);

            var earnAmtTxt = view.FindViewById<TextView>(Resource.Id.earn_trip_txt);

            var earnTrip = view.FindViewById<TextView>(Resource.Id.earn_trip_txt);

            withdrawBtn.Click += WithdrawBtn_Click;
            ChartTypespinner.Adapter = adapter;
            ChartTypespinner.ItemSelected += ChartTypespinner_ItemSelected;
            
        }

        private void WithdrawBtn_Click(object sender, System.EventArgs e)
        {
            
        }

        private void ChartTypespinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var index = e.Position;
            DrawChart(index);
        }

        private void DrawChart(int chartIndex)
        {
            List<Entry> DataEntry = new List<Entry>();
            DataEntry.Add(new Entry(100)
            {
                Label = days[0],
                ValueLabel = "100",
                Color = SKColor.Parse("#F44336")
            });

            DataEntry.Add(new Entry(200)
            {
                Label = days[1],
                ValueLabel = "200",
                Color = SKColor.Parse("#2196F3")
            });

            DataEntry.Add(new Entry(300)
            {
                Label = days[2],
                ValueLabel = "300",
                Color = SKColor.Parse("#673AB7")
            });

            DataEntry.Add(new Entry(400)
            {
                Label = days[3],
                ValueLabel = "400",
                Color = SKColor.Parse("#9C27B0")
            });

            DataEntry.Add(new Entry(500)
            {
                Label = days[4],
                ValueLabel = "500",
                Color = SKColor.Parse("#E91E63")
            });

            switch (chartIndex)
            {
                case 0:
                    var chartBar = new BarChart()
                    {
                        Entries = DataEntry,
                        LabelTextSize = 30.0f
                    };

                    ChartView.Chart = chartBar;
                    break;

                case 1:
                    var chartRader = new RadarChart()
                    {
                        Entries = DataEntry,
                        LabelTextSize = 30.0f
                    };

                    ChartView.Chart = chartRader;
                    break;

                case 2:
                    var chartPoint = new PointChart()
                    {
                        Entries = DataEntry,
                        LabelTextSize = 30.0f
                    };

                    ChartView.Chart = chartPoint;
                    break;

                case 3:
                    var chartGauge = new RadialGaugeChart()
                    {
                        Entries = DataEntry,
                        LabelTextSize = 30.0f
                    };

                    ChartView.Chart = chartGauge;
                    break;

                case 4:
                    var chartLine = new LineChart()
                    {
                        Entries = DataEntry,
                        LabelTextSize = 30.0f
                    };

                    ChartView.Chart = chartLine;
                    break;

                default:
                    var chart = new BarChart()
                    {
                        Entries = DataEntry,
                        LabelTextSize = 30.0f
                    };

                    ChartView.Chart = chart;
                    break;
            }

            
        }
    }
}