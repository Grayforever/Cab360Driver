using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;

namespace Cab360Driver.Fragments
{
    public class RatingsFragment : AndroidX.Fragment.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.rating, container, false);
            var viewDetailsBtn = view.FindViewById<MaterialButton>(Resource.Id.view_ratings_details_btn);
            viewDetailsBtn.Click += (s1, e1) =>
              {
                  Toast.MakeText(Activity, "Jennifer aseye", ToastLength.Long).Show();
              };

            return view;
        }
    }
}