using Android.Graphics;
using Android.OS;
using Android.Views;
using System;

namespace Cab360Driver.Fragments
{
    public class RatingsFragment : AndroidX.Fragment.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.rating, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            //var ratingView = view.FindViewById<RatingReviews>(Resource.Id.rating_reviews);

            int[] colors = new int[]{
                Color.ParseColor("#0e9d58"),
                Color.ParseColor("#bfd047"),
                Color.ParseColor("#ffc105"),
                Color.ParseColor("#ef7e14"),
                Color.ParseColor("#d36259")};

            int[] raters = new int[]{
                new Random().Next(100),
                new Random().Next(100),
                new Random().Next(100),
                new Random().Next(100),
                new Random().Next(100)
            };

            //string[] labels = new string[BarLabels.Stype1.Count];
            //BarLabels.Stype1.CopyTo(labels, 0);
            //ratingView.CreateRatingBars(100, labels, colors, raters);
        }
    }
}