using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
using AndroidX.RecyclerView.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Cab360Driver.Utils;
using Ramotion.CardSliderLib;
using System.Threading.Tasks;

namespace Cab360Driver.Fragments
{
    public class AccountFragment : AndroidX.Fragment.App.Fragment
    {
        private readonly int[] pics = { Resource.Drawable.music, Resource.Drawable.cool_car, Resource.Drawable.friendly, 
            Resource.Drawable.neat, Resource.Drawable.expert_nav };
        private readonly string[] compliments = { "Awesome music", "Cool car", "Made me laugh", 
            "Neat and tidy", "Expert navigation" };
        private string[] compliValues = new string[4];
        private SliderAdapter sliderAdapter => new SliderAdapter(pics, 5, OnCardClickListener);

        private CardSliderLayoutManager layoutManger;
        private RecyclerView recyclerView;
        private TextSwitcher complimentsSwitcher;
        private TextSwitcher compliValuesSwitcher;



        private int currentPosition;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        private async void GetDbAsync()
        {
            if(AppDataHelper.GetCurrentUser() == null)
            {
                return;
            }
            else
            {
                await Task.Run(() =>
                {
                    var compliRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{AppDataHelper.GetCurrentUser().Uid}/compliments");
                    compliRef.AddValueEventListener(new SingleValueListener(
                        snapshot =>
                        {
                            if (snapshot.Exists())
                            {
                                string cool_car = snapshot.Child("cool_car").Value.ToString();
                                string awesome_music = snapshot.Child("awesome_music").Value.ToString();
                                string friendly = snapshot.Child("made_me_laugh").Value.ToString();
                                string nav = snapshot.Child("expert_navigation").Value.ToString();
                                string neat = snapshot.Child("neat_and_tidy").Value.ToString();

                                compliValues = new string[]{ awesome_music, cool_car, friendly, neat, nav };
                                compliValuesSwitcher.SetCurrentText(compliValues[0]);
                            }
                        },
                       error =>
                       {
                           Toast.MakeText(Activity, error.Message, ToastLength.Short).Show();
                       }));
                    compliRef.KeepSynced(true);
                });
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.account, container, false);
            InitRecyclerView(view);
            InitSwitchers(view);
            GetDbAsync();
            return view;
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

        private void InitSwitchers(View view)
        {
            complimentsSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_compliments);
            complimentsSwitcher.SetFactory(new TextSwitcherUtil(Resource.Style.ComplimentsTextView, false, Activity));
            complimentsSwitcher.SetCurrentText(compliments[0]);

            compliValuesSwitcher = (TextSwitcher)view.FindViewById(Resource.Id.ts_compliValues);
            compliValuesSwitcher.SetFactory(new TextSwitcherUtil(Resource.Style.CompliValuesTextView, false, Activity));
            
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