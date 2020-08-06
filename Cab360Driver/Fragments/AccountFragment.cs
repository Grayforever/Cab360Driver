using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Palette.Graphics;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Cab360Driver.Adapters;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Cab360Driver.Utils;
using Ramotion.CardSliderLib;
using Refractored.Controls;
using System;
using static AndroidX.Palette.Graphics.Palette;

namespace Cab360Driver.Fragments
{
    public class AccountFragment : AndroidX.Fragment.App.Fragment
    {
        private readonly int[] pics = { Resource.Drawable.music, Resource.Drawable.cool_car, Resource.Drawable.friendly, Resource.Drawable.neat, Resource.Drawable.expert_nav };
        private readonly string[] compliments = { "Awesome music", "Cool car", "Made me laugh", "Neat and tidy", "Expert navigation" };
        private string[] compliValues = new string[4];

        private SliderAdapter sliderAdapter => new SliderAdapter(pics, 5, OnCardClickListener);
        public event EventHandler onQrClick;
        private CardSliderLayoutManager layoutManger;
        private RecyclerView recyclerView;
        private TextSwitcher complimentsSwitcher;
        private TextSwitcher compliValuesSwitcher;

        private int currentPosition;
        private TextView fullNameTv;
        private TextView drivingSinceTv;
        private ImageView settingsIv;
        private _BaseCircleImageView profileIv;
        private TextView ratingTv;
        private TextView ridesTv;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Palette.From(bitmap).Generate(new PaletteAsyncListener(palette=> 
            //{
            //    Palette.Swatch vibrant = palette.VibrantSwatch;
            //    if(vibrant != null)
            //    {
            //        fullNameTv.SetTextColor((ColorStateList)vibrant.TitleTextColor);
            //    }

            //})); 
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.account, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            recyclerView = (RecyclerView)view.FindViewById(Resource.Id.recycler_view);
            complimentsSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_compliments);
            compliValuesSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_compliValues);
            fullNameTv = view.FindViewById<TextView>(Resource.Id.account_username_tv);
            drivingSinceTv = view.FindViewById<TextView>(Resource.Id.account_jointime_tv);
            settingsIv = view.FindViewById<ImageView>(Resource.Id.account_settings_iv);
            profileIv = view.FindViewById<_BaseCircleImageView>(Resource.Id.acount_profile_iv);
            ratingTv = view.FindViewById<TextView>(Resource.Id.account_counter_tv);
            ridesTv = view.FindViewById<TextView>(Resource.Id.account_counter2_tv);

            //recycler
            recyclerView.SetAdapter(sliderAdapter);
            recyclerView.HasFixedSize = true;
            recyclerView.AddOnScrollListener(
                new MyRvOnScrollListener(
                    null,
                    (rv, newState) => {
                        if (newState == RecyclerView.ScrollStateIdle)
                            OnActiveCardChange();
                    })
                );
            layoutManger = (CardSliderLayoutManager)recyclerView.GetLayoutManager();
            recyclerView.SetLayoutManager(layoutManger);
            CardSnapHelper cardSnapHelper = new CardSnapHelper();
            cardSnapHelper.AttachToRecyclerView(recyclerView);

            //switcher
            complimentsSwitcher.SetFactory(new TextSwitcherUtil(Resource.Style.ComplimentsTextView, false, Activity));
            complimentsSwitcher.SetCurrentText(compliments[0]);
            compliValuesSwitcher.SetFactory(new TextSwitcherUtil(Resource.Style.CompliValuesTextView, false, Activity));

            var settingsBtn = view.FindViewById<ImageView>(Resource.Id.account_settings_iv);
            settingsBtn.Click += SettingsBtn_Click;
            var qrBtn = view.FindViewById<ImageView>(Resource.Id.account_qr_iv);
            qrBtn.Click += QrBtn_Click;
            SetDb();

        }

        private void QrBtn_Click(object sender, EventArgs e)
        {
            onQrClick?.Invoke(this, new EventArgs());
        }

        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            var ft = ChildFragmentManager.BeginTransaction();
            ft.Add(new SettingsFragment(), "settings");
            ft.CommitAllowingStateLoss();
        }

        private void SetDb()
        {
            switch (AppDataHelper.GetCurrentUser())
            {
                case null:
                    return;
                default:
                    {
                        var compliRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}");
                        compliRef.AddValueEventListener(new SingleValueListener(
                            snapshot =>
                            {
                                if (snapshot.Exists())
                                {
                                    fullNameTv.PostDelayed(() => 
                                    { 
                                        fullNameTv.Text = $"{snapshot.Child("fname").Value} {snapshot.Child("lname").Value}"; 
                                    }, 1000);

                                    drivingSinceTv.PostDelayed(() => 
                                    { 
                                        drivingSinceTv.Text = $"Driving since {DateTime.Parse(snapshot.Child("created_at").Value.ToString()).Year}"; 
                                    }, 1000);

                                    ratingTv.PostDelayed(() => 
                                    {
                                        ratingTv.Text = snapshot.Child("ratings").Value.ToString();
                                    }, 1000);
                                    

                                    Glide.With(Context)
                                    .Load(snapshot.Child("profile_img_url").Value.ToString())
                                    .Into(profileIv);

                                    string cool_car = snapshot.Child("compliments").Child("cool_car") == null ? "" : snapshot.Child("compliments").Child("cool_car").Value.ToString();
                                    string awesome_music = snapshot.Child("compliments").Child("awesome_music") == null ? "" : snapshot.Child("compliments").Child("awesome_music").Value.ToString();
                                    string friendly = snapshot.Child("compliments").Child("made_me_laugh") == null ? "" : snapshot.Child("compliments").Child("made_me_laugh").Value.ToString();
                                    string nav = snapshot.Child("compliments").Child("expert_navigation") == null ? "" : snapshot.Child("compliments").Child("expert_navigation").Value.ToString();
                                    string neat = snapshot.Child("compliments").Child("neat_and_tidy") == null ? "" : snapshot.Child("compliments").Child("neat_and_tidy").Value.ToString();
                                    
                                    compliValues = new string[] { awesome_music, cool_car, friendly, neat, nav };
                                    compliValuesSwitcher.PostDelayed(() => { compliValuesSwitcher.SetCurrentText(compliValues[0]); }, 1000);
                                    
                                }
                                else
                                {
                                    return;
                                }
                            },
                           error =>
                           {
                               Toast.MakeText(Activity, error.Message, ToastLength.Short).Show();
                           }));
                        compliRef.KeepSynced(true);
                        break;
                    }
            }
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

            if(compliValues.Length != 0)
            {
                compliValuesSwitcher.SetInAnimation(Activity, animV[0]);
                compliValuesSwitcher.SetOutAnimation(Activity, animV[1]);
                compliValuesSwitcher.SetText(compliValues[pos % compliValues.Length]);
            }
            else
            {
                return; 
            }
            currentPosition = pos;
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

        internal sealed class PaletteAsyncListener : Java.Lang.Object, IPaletteAsyncListener
        {
            private Action<Palette> _onGenerated;
            public PaletteAsyncListener(Action<Palette> onGenerated)
            {
                _onGenerated = onGenerated;
            }
            public void OnGenerated(Palette palette)
            {
                _onGenerated?.Invoke(palette);
            }
        }
    }
}