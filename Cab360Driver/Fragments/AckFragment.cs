using Android.OS;
using Android.Views;
using Google.Android.Material.Button;
using System;

namespace Cab360Driver.Fragments
{
    public class AckFragment : AndroidX.Fragment.App.Fragment
    {
        public event EventHandler OnSkip;
        public event EventHandler OnVisit;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.driver_ack_layout, container, false);
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            InitControls(view);
        }

        private void InitControls(View view)
        {
            var visit_btn = view.FindViewById<MaterialButton>(Resource.Id.visit_site_btn);
            var skip_btn = view.FindViewById<MaterialButton>(Resource.Id.skip_btn);
            skip_btn.Click += (s1, e1) =>
              {
                  OnSkip.Invoke(this, new EventArgs());
              };

            visit_btn.Click += (s2, e2) =>
              {
                  OnVisit.Invoke(this, new EventArgs());
              };
        }
    }
}