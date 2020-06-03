using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidX.Annotations;
using AndroidX.CardView.Widget;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;

using Cab360Driver.Activities;
using Cab360Driver.Adapters;
using Cab360Driver.Utils;
using Java.Lang;
using Ramotion.CardSliderLib;
using System;
using System.Windows.Markup;
using static Android.Views.ViewTreeObserver;
using static Cab360Driver.Fragments.AccountFragment.OnCardClickListener;
using static Cab360Driver.Utils.DecodeBitmapTask;

namespace Cab360Driver.Fragments
{
    public class AccountFragment : BaseFragment, IOnGlobalLayoutListener, IListener
    {
        private int[,] dotCoords = new int[5,2];
        private static int[] pics = { Resource.Drawable.p1, Resource.Drawable.p2, Resource.Drawable.p3, Resource.Drawable.p4, Resource.Drawable.p5 };
        private int[] maps = { Resource.Drawable.map_paris, Resource.Drawable.map_seoul, Resource.Drawable.map_london, Resource.Drawable.map_beijing, Resource.Drawable.map_greece };
        private int[] descriptions = { Resource.String.text1, Resource.String.text2, Resource.String.text3, Resource.String.text4, Resource.String.text5 };
        private string[] countries = { "PARIS", "SEOUL", "LONDON", "BEIJING", "THIRA" };
        private string[] places = { "The Louvre", "Gwanghwamun", "Tower Bridge", "Temple of Heaven", "Aegeana Sea" };
        private static string[] temperatures = { "21°C", "19°C", "17°C", "23°C", "20°C" };
        private static string[] times = { "Aug 1 - Dec 15    7:00-18:00", "Sep 5 - Nov 10    8:00-16:00", "Mar 8 - May 21    7:00-18:00" };
        private SliderAdapter sliderAdapter;

        private CardSliderLayoutManager layoutManger;
        private static RecyclerView recyclerView;
        private ImageSwitcher mapSwitcher;
        private static TextSwitcher temperatureSwitcher;
        private static TextSwitcher placeSwitcher;
        private static TextSwitcher clockSwitcher;
        private static TextSwitcher descriptionsSwitcher;
        private View greenDot;

        private TextView country1TextView;
        private TextView country2TextView;
        private int countryOffset1;
        private int countryOffset2;
        private long countryAnimDuration;
        private int currentPosition;

        private DecodeBitmapTask decodeMapBitmapTask;

        public override BaseFragment ProvideYourfragment()
        {
            return new AccountFragment();
        }

        public override View ProvideYourFragmentView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.account, container, false);
            sliderAdapter = new SliderAdapter(pics, 20, new OnCardClickListener(this));
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
            recyclerView.AddOnScrollListener(new OnScrollListener(this));
            layoutManger = (CardSliderLayoutManager)recyclerView.GetLayoutManager();
            recyclerView.SetLayoutManager(layoutManger);
            CardSnapHelper cardSnapHelper = new CardSnapHelper();
            cardSnapHelper.AttachToRecyclerView(recyclerView);
        }

        public void OnActiveCardChange()
        {
            int pos = layoutManger.ActiveCardPosition;
            if (pos == RecyclerView.NoPosition || pos == currentPosition)
            {
                return;
            }

            OnActiveCardChange(pos);
        }

        public void OnActiveCardChange(int pos)
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

            SetCountryText(countries[pos % countries.Length], left2right);

            temperatureSwitcher.SetInAnimation(Activity, animH[0]);
            temperatureSwitcher.SetOutAnimation(Activity, animH[1]);
            temperatureSwitcher.SetText(temperatures[pos % temperatures.Length]);

            placeSwitcher.SetInAnimation(Activity, animV[0]);
            placeSwitcher.SetOutAnimation(Activity, animV[1]);
            placeSwitcher.SetText(places[pos % places.Length]);

            clockSwitcher.SetInAnimation(Activity, animV[0]);
            clockSwitcher.SetOutAnimation(Activity, animV[1]);
            clockSwitcher.SetText(times[pos % times.Length]);

            descriptionsSwitcher.SetText(GetString(descriptions[pos % descriptions.Length]));

            ShowMap(maps[pos % maps.Length]);

            ViewCompat.Animate(greenDot)
                .TranslationX(dotCoords[pos % dotCoords.Length, 0])
                .TranslationY(dotCoords[pos % dotCoords.Length, 1])
                .Start();

            currentPosition = pos;
        }

        private void ShowMap([DrawableRes]int resId)
        {
            if (decodeMapBitmapTask != null)
            {
                decodeMapBitmapTask.Cancel(true);
            }

            int w = mapSwitcher.Width;
            int h = mapSwitcher.Height;

            decodeMapBitmapTask = new DecodeBitmapTask(Resources, resId, w, h, this);
            decodeMapBitmapTask.Execute();
        }

        private void SetCountryText(string text, bool left2right)
        {
            TextView invisibleText;
            TextView visibleText;
            if (country1TextView.Alpha > country2TextView.Alpha)
            {
                visibleText = country1TextView;
                invisibleText = country2TextView;
            }
            else
            {
                visibleText = country2TextView;
                invisibleText = country1TextView;
            }

            int vOffset;
            if (left2right)
            {
                invisibleText.SetX(0);
                vOffset = countryOffset2;
            }
            else
            {
                invisibleText.SetX(countryOffset2);
                vOffset = 0;
            }

            invisibleText.Text = text;

            ObjectAnimator iAlpha = ObjectAnimator.OfFloat(invisibleText, "alpha", 1f);
            ObjectAnimator vAlpha = ObjectAnimator.OfFloat(visibleText, "alpha", 0f);
            ObjectAnimator iX = ObjectAnimator.OfFloat(invisibleText, "x", countryOffset1);
            ObjectAnimator vX = ObjectAnimator.OfFloat(visibleText, "x", vOffset);

            AnimatorSet animSet = new AnimatorSet();
            animSet.PlayTogether(iAlpha, vAlpha, iX, vX);
            animSet.SetDuration(countryAnimDuration);
            animSet.Start();
        }

        private void InitCountryText(View view)
        {
            countryAnimDuration = Resources.GetInteger(Resource.Integer.labels_animation_duration);
            countryOffset1 = Resources.GetDimensionPixelSize(Resource.Dimension.left_offset);
            countryOffset2 = Resources.GetDimensionPixelSize(Resource.Dimension.card_width);
            country1TextView = (TextView)view.FindViewById(Resource.Id.tv_country_1);
            country2TextView = (TextView)view.FindViewById(Resource.Id.tv_country_2);

            country1TextView.SetX(countryOffset1);
            country2TextView.SetX(countryOffset2);
            country1TextView.Text = countries[0];
            country2TextView.Alpha = 0f;

            country1TextView.Typeface = Typeface.CreateFromAsset(Activity.Assets, "open-sans-extrabold.ttf");
            country2TextView.Typeface = Typeface.CreateFromAsset(Activity.Assets, "open-sans-extrabold.ttf");
        }

        private void InitSwitchers(View view)
        {
            temperatureSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_temperature);
            temperatureSwitcher.SetFactory(new TextViewFactoy(Resource.Style.TemperatureTextView, true, Activity));
            temperatureSwitcher.SetCurrentText(temperatures[0]);

            placeSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_place);
            placeSwitcher.SetFactory(new TextViewFactoy(Resource.Style.PlaceTextView, false, Activity));
            placeSwitcher.SetCurrentText(places[0]);

            clockSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_clock);
            clockSwitcher.SetFactory(new TextViewFactoy(Resource.Style.ClockTextView, false, Activity));
            clockSwitcher.SetCurrentText(times[0]);

            descriptionsSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_description);
            descriptionsSwitcher.SetInAnimation(Activity, Android.Resource.Animation.FadeIn);
            descriptionsSwitcher.SetOutAnimation(Activity, Android.Resource.Animation.FadeOut);
            descriptionsSwitcher.SetFactory(new TextViewFactoy(Resource.Style.DescriptionTextView, false, Activity));
            descriptionsSwitcher.SetCurrentText(GetString(descriptions[0]));

            mapSwitcher = (ImageSwitcher)view.FindViewById(Resource.Id.ts_map);
            mapSwitcher.SetInAnimation(Activity, Resource.Animation.fade_in);
            mapSwitcher.SetOutAnimation(Activity, Resource.Animation.fade_out);
            mapSwitcher.SetFactory(new ImageViewFactory(Activity));
            mapSwitcher.SetImageResource(maps[0]);
        }

        public void OnPostExecuted(Bitmap bitmap)
        {
            ((ImageView)mapSwitcher.NextView).SetImageBitmap(bitmap);
            mapSwitcher.ShowNext();
        }

        public class ImageViewFactory : Java.Lang.Object, ViewSwitcher.IViewFactory
        {
            Context context;
            public ImageViewFactory(Context context)
            {
                this.context = context;
            }

            public View MakeView()
            {
                ImageView imageView = new ImageView(context);
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);

                ViewGroup.LayoutParams lp = new ImageSwitcher.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                imageView.LayoutParameters = lp;

                return imageView;
            }
        }

        public class TextViewFactoy : Java.Lang.Object, ViewSwitcher.IViewFactory
        {
            int styleId;
            bool center;
            Context context;

            public TextViewFactoy([StyleRes] int styleId, bool center, Context context)
            {
                this.styleId = styleId;
                this.center = center;
                this.context = context;
            }

            public View MakeView()
            {
                TextView textView = new TextView(context);

                if (center)
                {
                    textView.Gravity = GravityFlags.Center;
                }

                textView.SetTextAppearance(styleId);
    
                return textView;
            }
        }

        private void InitGreenDot(View view)
        {
            greenDot = view.FindViewById(Resource.Id.green_dot);
            mapSwitcher.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }

        public void OnGlobalLayout()
        {
            mapSwitcher.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);

            int viewLeft = mapSwitcher.Left;
            int viewTop = mapSwitcher.Top + mapSwitcher.Height / 3;

            int border = 100;
            int xRange = System.Math.Max(1, mapSwitcher.Width - border * 2);
            int yRange = System.Math.Max(1, (mapSwitcher.Height / 3) * 2 - border * 2);

            Random rnd = new Random();

            for (int i = 0, cnt = dotCoords.Length; i < cnt; i++)
            {
                dotCoords[i, 0] = viewLeft + border + rnd.Next(xRange);
                dotCoords[i, 1] = viewTop + border + rnd.Next(yRange);
            }
            greenDot.SetX(dotCoords[0, 0]);
            greenDot.SetY(dotCoords[0,1]);
        }

        internal class OnCardClickListener : Java.Lang.Object, View.IOnClickListener
        {
            AccountFragment fragment;
            public OnCardClickListener(AccountFragment fragment)
            {
                this.fragment = fragment;
            }
            public void OnClick(View v)
            {
                CardSliderLayoutManager lm = (CardSliderLayoutManager)recyclerView.GetLayoutManager();

                if (lm.IsSmoothScrolling)
                {
                    return;
                }

                int activeCardPosition = lm.ActiveCardPosition;
                if (activeCardPosition == RecyclerView.NoPosition)
                {
                    return;
                }

                int clickedPosition = recyclerView.GetChildAdapterPosition(v);
                if (clickedPosition == activeCardPosition)
                {
                    Intent intent = new Intent(fragment.Activity, typeof(OnboardingActivity));
                    intent.PutExtra(Class.SimpleName, pics[activeCardPosition % pics.Length]);

                    CardView cardView = (CardView)v;
                    View sharedView = cardView.GetChildAt(cardView.ChildCount - 1);
                }
                else if (clickedPosition > activeCardPosition)
                {
                    recyclerView.SmoothScrollToPosition(clickedPosition);
                    fragment.OnActiveCardChange(clickedPosition);
                }
            }

            internal class OnScrollListener : RecyclerView.OnScrollListener
            {
                AccountFragment context;
                public OnScrollListener(AccountFragment context)
                {
                    this.context = context;
                }
                public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
                {
                    base.OnScrollStateChanged(recyclerView, newState);
                    if (newState == RecyclerView.ScrollStateIdle)
                    {
                        context.OnActiveCardChange();
                    }
                }
            }
        }
    }
}