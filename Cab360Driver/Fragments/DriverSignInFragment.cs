using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cab360Driver.Helpers;
using Google.Android.Material.Button;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.TextField;

namespace Cab360Driver.Fragments
{
    public class DriverSignInFragment : AndroidX.Fragment.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.driver_signin_layout, container, false);
            GetControls(view);
            return view;
        }

        private void GetControls(View view)
        {
            var emailText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signin_email_et);
            var passText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signin_pass_et);
            var forgotPassBtn = view.FindViewById<MaterialButton>(Resource.Id.btn_forgot_pass);
            forgotPassBtn.Click += (s1, e1) =>
            {
                ForgotPassFragment passFragment = new ForgotPassFragment();
                passFragment.Show(Activity.SupportFragmentManager, "show forgot fragment");
            };

            var continueFab = view.FindViewById<FloatingActionButton>(Resource.Id.fab1);
            continueFab.Click += (s2, e2) =>
            {
                bool enabled = Patterns.EmailAddress.Matcher(emailText.EditText.Text).Matches() && passText.EditText.Text.Length >= 8;
                switch (enabled)
                {
                    case false:
                        Toast.MakeText(Activity, "Please provide your account email and password", ToastLength.Short).Show();
                        break;
                    default:
                        {
                            var fireAuth = AppDataHelper.GetFirebaseAuth();
                            var fireDatabase = AppDataHelper.GetDatabase();
                            fireAuth.SignInWithEmailAndPassword(emailText.EditText.Text, passText.EditText.Text)
                                .AddOnSuccessListener(new OnSuccessListener(r =>
                                {
                                    var intent = new Intent(Activity, typeof(MainActivity));
                                    intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                                    StartActivity(intent);
                                }))
                                .AddOnFailureListener(new OnFailureListener(e => { Toast.MakeText(Activity, e.Message, ToastLength.Short).Show(); }));
                            break;
                        }
                }

            };
        }
    }
}