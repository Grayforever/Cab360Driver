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

        private int driverAnim = Resource.Drawable.driver_2;

        public readonly Action<EventArgs> _onSignUp;
        public readonly Action<EventArgs> _onSignIn;

        public OnboardingFragment(Action<EventArgs> onSignUp, Action<EventArgs> onSignIn)
        {
            _onSignUp = onSignUp;
            _onSignIn = onSignIn;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.driver_onboarding, container, false);
            Iview = view.FindViewById<ImageView>(Resource.Id.oboarding_gif);
            SignInBtn = view.FindViewById<MaterialButton>(Resource.Id.onbd_signin_btn);
            SignUpBtn = view.FindViewById<MaterialButton>(Resource.Id.onbd_signup_btn);
            SignInBtn.Click += SignInBtn_Click;
            SignUpBtn.Click += SignUpBtn_Click;

            Glide.With(this).Load(driverAnim).Into(Iview).ClearOnDetach();
            
            return view;
        }

        private void SignUpBtn_Click(object sender, EventArgs e)
        {
            _onSignUp.Invoke(e);
        }

        private void SignInBtn_Click(object sender, EventArgs e)
        {
            _onSignIn.Invoke(e);
        }
    }
}