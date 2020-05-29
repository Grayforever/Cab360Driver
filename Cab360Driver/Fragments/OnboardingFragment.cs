using Android.OS;
using Android.Views;
using Android.Widget;
using BumpTech.GlideLib;
using Google.Android.Material.Button;
using System;

namespace Cab360Driver.Fragments
{
    public class OnboardingFragment : AndroidX.Fragment.App.Fragment
    {
        private MaterialButton SignInBtn;
        private MaterialButton SignUpBtn;
        private ImageView Iview;
        public event EventHandler SignIn;
        public event EventHandler SignUp;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.driver_onboarding, container, false);
            SignInBtn = view.FindViewById<MaterialButton>(Resource.Id.onbd_signin_btn);
            SignUpBtn = view.FindViewById<MaterialButton>(Resource.Id.onbd_signup_btn);
            Iview = view.FindViewById<ImageView>(Resource.Id.oboarding_gif);
            SignInBtn.Click += SignInBtn_Click; SignUpBtn.Click += SignUpBtn_Click;
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            SetIntroBanner();
        }

        private void SetIntroBanner()
        {
            Glide.With(Activity)
                .Load(Resource.Drawable.driver_2)
                .Into(Iview);
        }

        private void SignUpBtn_Click(object sender, EventArgs e)
        {
            SignUp?.Invoke(this, new EventArgs());
        }

        private void SignInBtn_Click(object sender, EventArgs e)
        {
            SignIn?.Invoke(this, new EventArgs());
        }
    }
}