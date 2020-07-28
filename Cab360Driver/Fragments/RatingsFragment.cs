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
            View view = inflater.Inflate(Resource.Layout.rating, container, false);
            return view;
        }
    }
}