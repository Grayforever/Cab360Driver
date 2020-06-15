using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
using AndroidX.Interpolator.View.Animation;
using AndroidX.RecyclerView.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.EventListeners;
using Cab360Driver.Utils;
using Google.Android.Material.AppBar;
using Ramotion.CardSliderLib;
using System;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Cab360Driver.Fragments
{
    [Register("id.Cab360Driver.Fragments.AccountFragment")]
    public class AccountFragment : AndroidX.Fragment.App.Fragment
    {
        private readonly int[] pics = { Resource.Drawable.cool_car, Resource.Drawable.cool_car, Resource.Drawable.made_me_laugh, Resource.Drawable.cool_car, Resource.Drawable.cool_car };
        
        private readonly string[] compliments = { "Awesome music", "Cool car", "Made me laugh", "Neat and tidy", "Expert navigation" };
        
        private SliderAdapter sliderAdapter => new SliderAdapter(pics, 20, OnCardClickListener);

        private CardSliderLayoutManager layoutManger;
        private RecyclerView recyclerView;
        private TextSwitcher complimentsSwitcher;
        private AppBarLayout appbar;
        private Toolbar toolbar;
        private ProfileHeaderBitmap headerBitmap;
        private LinearLayout titleContainer;
        private FrameLayout mHeaderLayout;
        private Bitmap defaultBlurProfileBitmap;
        private TextView profile_title_user;
        private TextView profileTitleCount;

        private bool collapsed = false;
        private bool titleShow = false;
        private ValueAnimator valueAnimator = null;

        private int currentPosition;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.account, container, false);
            appbar = view.FindViewById<AppBarLayout>(Resource.Id.appbar_account);
            toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar_account);
            headerBitmap = view.FindViewById<ProfileHeaderBitmap>(Resource.Id.profile_header_bg);
            titleContainer = view.FindViewById<LinearLayout>(Resource.Id.profile_title_container);
            titleContainer.Visibility = ViewStates.Invisible;
            mHeaderLayout = view.FindViewById<FrameLayout>(Resource.Id.header_layout);
            profile_title_user = view.FindViewById<TextView>(Resource.Id.profile_title_user);
            profileTitleCount = view.FindViewById<TextView>(Resource.Id.profile_title_count);
            SetDefaultProfileBg(Resource.Drawable.ic_bg);
            InitListener();
            InitRecyclerView(view);
            InitSwitchers(view);
            return view;
        }

        private void InitListener()
        {
            float parallaxMultiplier = ((CollapsingToolbarLayout.LayoutParams)headerBitmap.LayoutParameters).ParallaxMultiplier;

            appbar.OffsetChanged += (s1, e1) =>
            {
                int totalScrollRange = e1.AppBarLayout.TotalScrollRange;
                AnimalOnTitle(e1.VerticalOffset, totalScrollRange);
                AnimalOnAvatar(e1.VerticalOffset, totalScrollRange, parallaxMultiplier);
                AnimalOnBg(e1.VerticalOffset, totalScrollRange);
            };
        }

        private void AnimalOnBg(int verticalOffset, int totalScrollRange)
        {
            float abs = Math.Abs(verticalOffset) / totalScrollRange;
            headerBitmap.Post(() => 
            {
                headerBitmap.SetForegroundAlpha(abs);
            });
        }

        private void AnimalOnAvatar(int i, int i2, float parallaxMultiplier)
        {
            float f = i2;
            if (((int)(parallaxMultiplier * f)) + i <= 0 && !collapsed)
            {
                collapsed = true;
                mHeaderLayout.Animate().Cancel();
                mHeaderLayout
                    .Animate()
                    .ScaleX(0.0f)
                    .ScaleY(0.0f)
                    .Alpha(0.0f)
                    .SetDuration(200)
                    .SetInterpolator(new FastOutLinearInInterpolator())
                    .SetListener(new AnimatorListener(animation =>
                    {
                        if (collapsed)
                        {
                            mHeaderLayout.Visibility = ViewStates.Invisible;
                        }
                    }, null, null, null)).Start();
            }
            else if (((int)(f * parallaxMultiplier)) + i >= 0 && collapsed)
            {
                collapsed = false;
                mHeaderLayout.Visibility = ViewStates.Visible;
                mHeaderLayout
                    .Animate()
                    .ScaleX(1.0f)
                    .ScaleY(1.0f)
                    .Alpha(1.0f)
                    .SetDuration(200)
                    .SetInterpolator(new LinearOutSlowInInterpolator())
                    .SetListener(new AnimatorListener(null, null, null, null))
                    .Start();
            }
        }
    
        private void AnimalOnTitle(int i, int i2)
        {
            if (valueAnimator == null)
            {
                valueAnimator = (ValueAnimator)ValueAnimator.OfFloat(1.0f, 0.0f).SetDuration(600);
                valueAnimator.AddUpdateListener(new AnimatorUpdateListener
                    (valueAnimator =>
                    {
                        float floatValue = (float)valueAnimator.AnimatedValue;
                        profile_title_user.TranslationY = profile_title_user.Height * floatValue;
                        profileTitleCount.TranslationY = profile_title_user.Height * floatValue;
                        float f = 1.0f - (floatValue * 0.2f);
                        profile_title_user.Alpha = f;
                        profileTitleCount.Alpha = f;
                    }));
            }

            if (i2 + i == 0)
            {
                if (!titleShow)
                {
                    titleShow = true;
                    valueAnimator.Cancel();
                    valueAnimator.RemoveAllListeners();
                    valueAnimator.AddListener(new AnimatorListener(
                        anim1 =>
                        {
                            titleContainer.Visibility = ViewStates.Visible;
                        }, null, null, 
                        anim2=>
                        {
                            profile_title_user.TranslationY = (profile_title_user.Height / 2);
                            profile_title_user.Alpha = 0.2f;
                            profileTitleCount.TranslationY = (profile_title_user.Height / 2);
                            profileTitleCount.Alpha = 0.2f;
                            titleContainer.Visibility = ViewStates.Visible;
                        }));
                    valueAnimator.Start();
                }
            }
            else if (titleShow)
            {
                titleShow = false;
                valueAnimator.Cancel();
                valueAnimator.RemoveAllListeners();
                valueAnimator.AddListener(new AnimatorListener(
                    anim1 =>
                    {
                        titleContainer.Visibility = ViewStates.Gone;
                    }, null, null,
                    anim2=> 
                    {
                        titleContainer.Visibility = ViewStates.Visible;
                    }));
                valueAnimator.Reverse();
            }
        }
                
        public int Dip2px(float f)
        {
            return (int)(Resources.DisplayMetrics.Density * f);
        }

        private void SetDefaultProfileBg(int i)
        {
            try
            {
                headerBitmap.SetImageResource(i);
                if (defaultBlurProfileBitmap == null)
                {
                    int width = Resources.DisplayMetrics.WidthPixels;
                    defaultBlurProfileBitmap = Blur.GausianBlur(Activity, Bitmap.CreateScaledBitmap(BitmapFactory.DecodeResource(Resources, i),
                            width / 15, Dip2px(275.0f) / 15, false), 25);
                }
                if (defaultBlurProfileBitmap != null)
                {
                    headerBitmap.SetForeground(new BitmapDrawable(defaultBlurProfileBitmap));
                    headerBitmap.SetForegroundAlpha(0.0f);
                }
            }
            catch (Exception e)
            {
                Log.Debug("error", e.Message);
            }
        }

        private void InitRecyclerView(View view)
        {
            recyclerView = (RecyclerView)view.FindViewById(Resource.Id.recycler_view);
            recyclerView.SetAdapter(sliderAdapter);
            recyclerView.HasFixedSize = true;
            recyclerView.AddOnScrollListener(
                new MyRvOnScrollListener(
                    null,
                    (rv, newState)=> {
                        if (newState == RecyclerView.ScrollStateIdle)
                            OnActiveCardChange();
                    })
                );
            layoutManger = (CardSliderLayoutManager)recyclerView.GetLayoutManager();
            recyclerView.SetLayoutManager(layoutManger);
            new CardSnapHelper().AttachToRecyclerView(recyclerView);
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

            complimentsSwitcher.SetInAnimation(Activity, animV[0]);
            complimentsSwitcher.SetOutAnimation(Activity, animV[1]);
            complimentsSwitcher.SetText(compliments[pos % compliments.Length]);

            currentPosition = pos;
        }

        private void InitSwitchers(View view)
        {
            complimentsSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_compliments);
            complimentsSwitcher.SetFactory(new TextViewFactoy(Resource.Style.ComplimentsTextView, false, Activity));
            complimentsSwitcher.SetCurrentText(compliments[0]);

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


        private View.IOnClickListener OnCardClickListener => new MyViewOnClickListener(
            v =>
            {
                var lm = (CardSliderLayoutManager)recyclerView.GetLayoutManager();

                if (lm.IsSmoothScrolling)
                    return;

                var activeCardPosition = lm.ActiveCardPosition;
                if (activeCardPosition == RecyclerView.NoPosition)
                    return;

                var clickedPosition = recyclerView.GetChildAdapterPosition(v);
                if (clickedPosition == activeCardPosition)
                {
                    
                }
                else if (clickedPosition > activeCardPosition)
                {
                    recyclerView.SmoothScrollToPosition(clickedPosition);
                    OnActiveCardChange(clickedPosition);
                }
            }
        );

        
    }
}