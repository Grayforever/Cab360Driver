using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Cab360Driver.Adapters;
using Microcharts;
using Microcharts.Droid;
using SkiaSharp;
using System.Collections.Generic;

namespace Cab360Driver.Fragments
{
    public class EarningsFragment : BaseFragment
    {
        public override BaseFragment ProvideYourfragment()
        {
            return new EarningsFragment();
        }
        private enum ChartType
        {
            BarChart,
            RadarChart,
            PointChart,
            DonutChart,
            LineChart
        }
        private string[] chartArray = { "Bar Chart", "Radar Chart", "Point Chart", "Radial Chart", "Line Chart"};

        private ChartView ChartView;

        public override View ProvideYourFragmentView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.earnings, container, false);
            InitControls(view);
            return view;
        }

        private void InitControls(View view)
        {
            ChartView = view.FindViewById<ChartView>(Resource.Id.earn_chartview);
            var ChartTypespinner = view.FindViewById<AppCompatSpinner>(Resource.Id.chart_type_spinner);
            var adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, chartArray);
            ChartTypespinner.Adapter = adapter;
            ChartTypespinner.ItemSelected += ChartTypespinner_ItemSelected;
            DrawChart(0);
        }

        private void ChartTypespinner_ItemSelected(object sender, Android.Widget.AdapterView.ItemSelectedEventArgs e)
        {
            var index = e.Position;
            DrawChart(index);
        }

        private void DrawChart(int chartIndex)
        {
            List<Entry> DataEntry = new List<Entry>();
            DataEntry.Add(new Entry(100)
            {
                Label = "Monday",
                ValueLabel = "100",
                Color = SKColor.Parse("#F44336")
            });

            DataEntry.Add(new Entry(200)
            {
                Label = "Tuesday",
                ValueLabel = "200",
                Color = SKColor.Parse("#2196F3")
            });

            DataEntry.Add(new Entry(300)
            {
                Label = "Wednesday",
                ValueLabel = "300",
                Color = SKColor.Parse("#673AB7")
            });

            DataEntry.Add(new Entry(400)
            {
                Label = "Thursday",
                ValueLabel = "400",
                Color = SKColor.Parse("#9C27B0")
            });

            DataEntry.Add(new Entry(500)
            {
                Label = "Friday",
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