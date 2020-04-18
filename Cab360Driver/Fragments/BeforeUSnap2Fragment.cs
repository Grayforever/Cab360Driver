using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace Cab360Driver.Fragments
{
    public class BeforeUSnap2Fragment : AndroidX.Fragment.App.DialogFragment
    {
        public event EventHandler StartLicenseCamera;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.Theme_AppCompat_Light_DialogWhenLarge);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return inflater.Inflate(Resource.Layout.licence_intro_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            GetControls(view);
        }

        private void GetControls(View view)
        {
            var startCameraBtn = view.FindViewById<Button>(Resource.Id.intro_takelicense_btn1);
            startCameraBtn.Click += StartCameraBtn_Click;
        }

        private void StartCameraBtn_Click(object sender, EventArgs e)
        {
            StartLicenseCamera.Invoke(this, new EventArgs());
        }
    }
}