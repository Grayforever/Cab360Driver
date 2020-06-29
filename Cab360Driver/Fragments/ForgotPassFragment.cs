using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Cab360Driver.Helpers;
using Google.Android.Material.Button;
using Google.Android.Material.Snackbar;
using Google.Android.Material.TextField;

namespace Cab360Driver.Fragments
{
    public class ForgotPassFragment : AndroidX.Fragment.App.DialogFragment
    {
        private ConstraintLayout forgotPassRoot;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.AppTheme_DialogWhenLarge);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.forgot_password, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            forgotPassRoot = view.FindViewById<ConstraintLayout>(Resource.Id.forgot_pass_root);
            var emailEditText = view.FindViewById<TextInputLayout>(Resource.Id.forgot_email_et);
            var nextBtn = view.FindViewById<MaterialButton>(Resource.Id.btn_fgt_email_snd);
            emailEditText.EditText.TextChanged += (s1, e1) =>
            {
                nextBtn.Enabled = Patterns.EmailAddress.Matcher(emailEditText.EditText.Text).Matches();
            };

            nextBtn.Click += (s1, e1) =>
            {
                AppDataHelper.GetFirebaseAuth().SendPasswordResetEmail(emailEditText.EditText.Text)
                .AddOnCompleteListener(new OnCompleteListener(t =>
                {
                    if (t.IsSuccessful)
                    {
                        Snackbar.Make(forgotPassRoot, "We've sent you a confirmation message", BaseTransientBottomBar.LengthIndefinite)
                            .SetAction("OK", view =>
                            {
                                Toast.MakeText(Activity, "clicked", ToastLength.Short).Show();
                            });
                    }
                    else
                    {
                        Toast.MakeText(Activity, t.Exception.Message, ToastLength.Short).Show();
                    }

                }));
            };
        }
    }
}