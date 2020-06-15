using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.EnumsConstants;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.Util;
using System;

namespace Cab360Driver.Fragments
{
    public class DriverRegisterFragment : AndroidX.Fragment.App.Fragment
    {
        private TextInputLayout FnameText, LnameText, EmailText, PhoneText, PassText, CodeText;
        private AppCompatAutoCompleteTextView CityText;
        private MaterialButton SubmitBtn;
        private string[] names = { "Accra", "Kumasi", "Takoradi", "Ho", "Tema", "Tamale" };
        private FirebaseAuth FireAuth;
        private DatabaseReference driverRef;

        public event EventHandler<SignUpSuccessArgs> SignUpSuccess;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view =  inflater.Inflate(Resource.Layout.driver_signup_layout, container, false);
            GetControls(view);
            return view;
        }

        private void GetControls(View view)
        {
            SubmitBtn = view.FindViewById<MaterialButton>(Resource.Id.drv_signup_sbmtbtn);
            FnameText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_fname_et);
            LnameText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_lname_et);
            EmailText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_email_et);
            PhoneText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_phone_et);
            CodeText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_code_et);
            PassText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_pass_et);
            CityText = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.autocity_et);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            FnameText.EditText.AfterTextChanged += EditText_AfterTextChanged;
            LnameText.EditText.AfterTextChanged += EditText_AfterTextChanged;
            PassText.EditText.AfterTextChanged += EditText_AfterTextChanged;
            EmailText.EditText.AfterTextChanged += EditText_AfterTextChanged;
            PhoneText.EditText.AfterTextChanged += EditText_AfterTextChanged;

            var adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, names);
            CityText.Adapter = adapter;
            CityText.AfterTextChanged += EditText_AfterTextChanged;

            SubmitBtn.Click += (s1, e1) =>
            {
                FireAuth = AppDataHelper.GetFirebaseAuth();
                FireAuth.CreateUserWithEmailAndPassword(EmailText.EditText.Text, PassText.EditText.Text)
                    .AddOnSuccessListener(new OnSuccessListener(r=> 
                    {
                        HashMap driverMap = new HashMap();
                        driverMap.Put("firstname", FnameText.EditText.Text);
                        driverMap.Put("lastname", LnameText.EditText.Text);
                        driverMap.Put("email", EmailText.EditText.Text); ;
                        driverMap.Put("phone", PhoneText.EditText.Text);
                        driverMap.Put("city", CityText.Text);
                        driverMap.Put("invitecode", CodeText.EditText.Text);
                        driverMap.Put("created_at", DateTime.UtcNow.ToString());
                        driverMap.Put("isPartner", "false");
                        driverMap.Put("stage_of_registration", RegistrationStage.Registration.ToString());

                        SaveDriverToDb(driverMap);
                    }))
                    .AddOnFailureListener(new OnFailureListener(e=> { Toast.MakeText(Activity, e.Message, ToastLength.Short).Show(); }));
            };
        }

        private void EditText_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            CheckIfEmpty();
        }

        private void SaveDriverToDb(HashMap driverMap)
        {
            driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid);
            driverRef.SetValue(driverMap)
                .AddOnSuccessListener(new OnSuccessListener(r=> 
                {
                    driverRef.Child("stage_of_registration").SetValue(RegistrationStage.Partnering.ToString())
                        .AddOnSuccessListener(new OnSuccessListener(r2=> 
                        {
                            SignUpSuccess.Invoke(this, new SignUpSuccessArgs { IsCompleted = true });
                        }))
                        .AddOnFailureListener(new OnFailureListener(e1=> { Toast.MakeText(Activity, e1.Message, ToastLength.Short).Show(); }));
                }))
                .AddOnFailureListener(new OnFailureListener(e=> { Toast.MakeText(Activity, e.Message, ToastLength.Short).Show(); }));
        }

        private void CheckIfEmpty()
        {
            SubmitBtn.Enabled = Patterns.EmailAddress.Matcher(EmailText.EditText.Text).Matches() && FnameText.EditText.Text.Length >= 3 &&
                LnameText.EditText.Text.Length >= 3 && CityText.Text.Length >= 2 && PhoneText.EditText.Text.Length >= 8 && PassText.EditText.Text.Length >= 8;
        }

        public class SignUpSuccessArgs : EventArgs
        {
            public bool IsCompleted { get; set; }
        }

    }
}