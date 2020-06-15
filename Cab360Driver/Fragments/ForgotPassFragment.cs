using Android.Gms.Tasks;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cab360Driver.Helpers;
using Google.Android.Material.Button;
using Google.Android.Material.Snackbar;
using Google.Android.Material.TextField;

namespace Cab360Driver.Fragments
{
    public class ForgotPassFragment : AndroidX.Fragment.App.DialogFragment, IOnSuccessListener, IOnFailureListener
    {
        private ScrollView forgotPassRoot;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            
            var view = inflater.Inflate(Resource.Layout.forgot_password, container, false);
            forgotPassRoot = view.FindViewById<ScrollView>(Resource.Id.forgot_pass_root);
            var emailEditText = view.FindViewById<TextInputLayout>(Resource.Id.forgot_email_et);
            var nextBtn = view.FindViewById<MaterialButton>(Resource.Id.btn_fgt_email_snd);
            emailEditText.EditText.TextChanged += (s1, e1) =>
            {
                nextBtn.Enabled = Patterns.EmailAddress.Matcher(emailEditText.EditText.Text).Matches();
            };
            
            nextBtn.Click += (s1, e1) =>
            {
                var fireAuth = AppDataHelper.GetFirebaseAuth();
                fireAuth.SendPasswordResetEmail(emailEditText.EditText.Text)
                  .AddOnSuccessListener(this)
                  .AddOnFailureListener(this);
            };
            return view;
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Toast.MakeText(Activity, e.Message, ToastLength.Short).Show();
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            Snackbar.Make(forgotPassRoot, "We've sent you a confirmation message", BaseTransientBottomBar.LengthIndefinite)
                .SetAction("OK",view =>
                {
                    Toast.MakeText(Activity, "clicked", ToastLength.Short).Show();
                });
        }
    }
}