using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Google.Android.Material.Button;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.TextField;
using System.Text.RegularExpressions;

namespace Cab360Driver.Fragments
{
    public class DriverSignInFragment : AndroidX.Fragment.App.Fragment
    {
        private FirebaseAuth _fireAuth;
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("driverInfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _fireAuth = AppDataHelper.GetFirebaseAuth();
            editor = preferences.Edit();

            bool isValid = CreteRegex("GR-4945-12");
            Log.Debug("regex", isValid.ToString());
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
                            Log.Info("sign_in_btn_test", "button enabled");
                            _fireAuth.SignInWithEmailAndPassword(emailText.EditText.Text, passText.EditText.Text)
                                .AddOnSuccessListener(new OnSuccessListener(r =>
                                {
                                    var fireDb = AppDataHelper.GetDatabase();
                                    var dataRef = fireDb.GetReference("Drivers" + _fireAuth.CurrentUser.Uid);
                                    dataRef.AddValueEventListener(new SingleValueListener(d =>
                                    {
                                        if (!d.Exists())
                                            return;

                                        string fname, lname, email, phone, city;
                                        fname = (d.Child("fname") != null) ? d.Child("fname").Value.ToString() : "";
                                        lname = (d.Child("lname") != null) ? d.Child("lname").Value.ToString() : "";
                                        email = (d.Child("email") != null) ? d.Child("email").Value.ToString() : "";
                                        phone = (d.Child("phone") != null) ? d.Child("phone").Value.ToString() : "";
                                        city = (d.Child("city") != null) ? d.Child("city").Value.ToString() : "";

                                        editor.PutString("fname", fname);
                                        editor.PutString("lname", lname);
                                        editor.PutString("email", email);
                                        editor.PutString("phone", phone);
                                        editor.PutString("city", city);
                                        editor.Apply();

                                    }, e=> { Toast.MakeText(Activity, e.Message, ToastLength.Short).Show(); }));
                                }))
                                .AddOnFailureListener(new OnFailureListener(e => { Toast.MakeText(Activity, e.Message, ToastLength.Short).Show(); }));
                            break;
                        }
                }

            };
        }

        private bool CreteRegex(string test)
        {
            Regex regex = new Regex(@"^[a-zA-Z]{2}-\d+\-(\d{2}|[a-zA-Z])$");
            return regex.IsMatch(test);
        }
    }
}