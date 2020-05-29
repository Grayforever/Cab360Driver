using Android;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.Fragments;
using Cab360Driver.Utils;
using Ramotion.CardSliderLib;
using System;

namespace Cab360DriveResource.Fragments
{
    public class AccountFragment : BaseFragment
    {
        private int[][] dotCoords = [5][2];
        private static int[] pics = { Resource.Drawable.p1, Resource.Drawable.p2, Resource.Drawable.p3, Resource.Drawable.p4, Resource.Drawable.p5 };
        private int[] maps = { Resource.Drawable.map_paris, Resource.Drawable.map_seoul, Resource.Drawable.map_london, Resource.Drawable.map_beijing, Resource.Drawable.map_greece };
        private int[] descriptions = { Resource.String.text1, Resource.String.text2, Resource.String.text3, Resource.String.text4, Resource.String.text5 };
        private string[] countries = { "PARIS", "SEOUL", "LONDON", "BEIJING", "THIRA" };
        private string[] places = { "The Louvre", "Gwanghwamun", "Tower Bridge", "Temple of Heaven", "Aegeana Sea" };
        private static string[] temperatures = { "21°C", "19°C", "17°C", "23°C", "20°C" };
        private static string[] times = { "Aug 1 - Dec 15    7:00-18:00", "Sep 5 - Nov 10    8:00-16:00", "Mar 8 - May 21    7:00-18:00" };
        private SliderAdapter sliderAdapter = new SliderAdapter(pics, 20, new OnCardClickListener());

        private CardSliderLayoutManager layoutManger;
        private RecyclerView recyclerView;
        private ImageSwitcher mapSwitcher;
        private static TextSwitcher temperatureSwitcher;
        private static TextSwitcher placeSwitcher;
        private static TextSwitcher clockSwitcher;
        private static TextSwitcher descriptionsSwitcher;
        private View greenDot;

        private DecodeBitmapTask decodeMapBitmapTask;
        private DecodeBitmapTask.IListener mapLoadListener;

        public override BaseFragment ProvideYourfragment()
        {
            return new AccountFragment();
        }

        public override View ProvideYourFragmentView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.account, container, false);
            InitRecyclerView(view);
            InitCountryText(view);
            InitSwitchers(view);
            InitGreenDot(view);
            return view;
        }

        private void InitRecyclerView(View view)
        {
            recyclerView = (RecyclerView)view.FindViewById(Resource.Id.recycler_view);
            recyclerView.SetAdapter(sliderAdapter);
            recyclerView.HasFixedSize = true;
            recyclerView.AddOnScrollListener(new OnScrollListener());
            layoutManger = (CardSliderLayoutManager)recyclerView.GetLayoutManager();

            new CardSnapHelper().AttachToRecyclerView(recyclerView);
        }

        public void OnActiveCardChange()
        {
            int[] animH = { Resource.Animation.slide_in_right, Resource.Animation.slide_out_left };
            int[] animV = { Resource.Animation.slide_in_top, Resource.Animation.slide_out_bottom };

            bool left2right = pos < currentPosition;
            if (left2right)
            {
                animH[0] = Resource.Animation.slide_in_left;
                animH[1] = Resource.Animation.slide_out_right;

                animV[0] = Resource.Animation.slide_in_bottom;
                animV[1] = Resource.Animation.slide_out_top;
            }

            SetCountryText(countries[pos % countries.length], left2right);

            temperatureSwitcher.SetInAnimation(MainActivity.this, animH[0]);
            temperatureSwitcher.SetOutAnimation(MainActivity.this, animH[1]);
            temperatureSwitcher.SetText(temperatures[pos % temperatures.length]);

            placeSwitcher.SetInAnimation(MainActivity.this, animV[0]);
            placeSwitcher.SetOutAnimation(MainActivity.this, animV[1]);
            placeSwitcher.SetText(places[pos % places.length]);

            clockSwitcher.SetInAnimation(Activity, animV[0]);
            clockSwitcher.SetOutAnimation(Activity, animV[1]);
            clockSwitcher.SetText(times[pos % times.length]);

            descriptionsSwitcher.SetText(getString(descriptions[pos % descriptions.length]));

            showMap(maps[pos % maps.Length]);

            ViewCompat.Animate(greenDot)
                    .TranslationX(dotCoords[pos % dotCoords.Length][0])
                    .TranslationY(dotCoords[pos % dotCoords.Length][1])
                    .Start();

            currentPosition = pos;
        }

        private void InitCountryText(View view)
        {
            throw new NotImplementedException();
        }

        private void InitSwitchers(View view)
        {
            throw new NotImplementedException();
        }

        private void InitGreenDot(View view)
        {
            throw new NotImplementedException();
        }

        protected internal class OnScrollListener: RecyclerView.OnScrollListener
        {
            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {
                base.OnScrollStateChanged(recyclerView, newState);
                if (newState == RecyclerView.ScrollStateIdle)
                {
                    OnActiveCardChange();
                }
            }

            
        }
    }
}