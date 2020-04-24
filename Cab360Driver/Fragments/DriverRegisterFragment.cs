using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Cab360Driver.DataModels;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Google.Android.Material.Button;
using Google.Android.Material.Snackbar;
using Google.Android.Material.TextField;
using Java.Lang;
using System;
using static Android.Views.View;

namespace Cab360Driver.Fragments
{
    public class DriverRegisterFragment : AndroidX.Fragment.App.Fragment, IOnKeyListener, ITextWatcher
    {
        private TextInputLayout FnameText, LnameText, EmailText, PhoneText, PassText, CodeText;
        private AppCompatAutoCompleteTextView CityText;
        private MaterialButton SubmitBtn;
        private Driver DriverPersonal;
        private CoordinatorLayout driverSignupRoot;
        private FirebaseDatabase FireDatabase;
        private FirebaseAuth FireAuth;
        private TaskCompletionListeners TaskCompletionListener = new TaskCompletionListeners();


        public class StageOnePassEventArgs : EventArgs
        {
            public Driver driver { get; set; }
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            //var api_key = GetString(Resource.String.mapKey);

            //if (!PlacesApi.IsInitialized)
            //{
            //    PlacesApi.Initialize(Activity, api_key);
            //}
        }

        public event EventHandler<StageOnePassEventArgs> StageOnePassEvent;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            FireDatabase = AppDataHelper.GetDatabase();
            FireAuth = AppDataHelper.GetFirebaseAuth();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view =  inflater.Inflate(Resource.Layout.driver_signup_layout, container, false);
            GetControls(view);
            return view;
        }

        private void GetControls(View view)
        {
            driverSignupRoot = view.FindViewById<CoordinatorLayout>(Resource.Id.drv_signup_root);

            //button
            SubmitBtn = view.FindViewById<MaterialButton>(Resource.Id.drv_signup_sbmtbtn);
            SubmitBtn.Click += SubmitBtn_Click;

            //edittexts
            FnameText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_fname_et);
            FnameText.SetOnKeyListener(this);
            FnameText.EditText.AddTextChangedListener(this);

            LnameText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_lname_et);
            LnameText.SetOnKeyListener(this);
            LnameText.EditText.AddTextChangedListener(this);

            EmailText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_email_et);
            EmailText.SetOnKeyListener(this);
            EmailText.EditText.AddTextChangedListener(this);

            PhoneText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_phone_et);
            PhoneText.SetOnKeyListener(this);
            PhoneText.EditText.AddTextChangedListener(this);

            CodeText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_code_et);

            PassText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_pass_et);
            PassText.EditText.AddTextChangedListener(this);
            PassText.SetOnKeyListener(this);

            var names = new string[] { "Accra", "Kumasi", "Taadi" };
            Android.Widget.ArrayAdapter arrayAdapter = new Android.Widget.ArrayAdapter<string>(Activity, Resource.Layout.support_simple_spinner_dropdown_item, names);

            CityText = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.autocity_et);
            CityText.Adapter = arrayAdapter;
            CityText.SetOnKeyListener(this);
            CityText.AddTextChangedListener(this);
        }

        private void CheckIfEmpty()
        {
            var email = EmailText.EditText.Text;
            var fname = FnameText.EditText.Text;
            var lname = LnameText.EditText.Text;
            var city = CityText.Text;
            var code = CodeText.EditText.Text;
            var phone = PhoneText.EditText.Text;
            var pass = PassText.EditText.Text;

            SubmitBtn.Enabled = Android.Util.Patterns.EmailAddress.Matcher(email).Matches() && fname.Length >= 3 && lname.Length >= 3 && city.Length >=2 && phone.Length >=8
                && pass.Length >=8;
        }

        private void SubmitBtn_Click(object sender, EventArgs e)
        {
            DriverPersonal = new Driver
            {
                Fname = FnameText.EditText.Text,
                Lname = LnameText.EditText.Text,
                Phone = PhoneText.EditText.Text,
                City = CityText.Text,
                Code = CodeText.EditText.Text,
                Email = EmailText.EditText.Text,
                IsPartner = "0"

            };

            FireAuth.CreateUserWithEmailAndPassword(DriverPersonal.Email, PassText.EditText.Text)
                .AddOnSuccessListener(TaskCompletionListener)
                .AddOnFailureListener(TaskCompletionListener);

            TaskCompletionListener.Successful += (s1, e1) =>
            {
                StageOnePassEvent?.Invoke(this, new StageOnePassEventArgs { driver = DriverPersonal });
            };

            TaskCompletionListener.Failure += (s2, e2) =>
            {
                Android.Widget.Toast.MakeText(Activity, "Error", Android.Widget.ToastLength.Long).Show();
            };


        }

        public bool OnKey(View v, [GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            var action = e.Action;
            if (action == KeyEventActions.Up)
            {
                CheckIfEmpty();
            }
            return false;
        }

        public void AfterTextChanged(IEditable s)
        {
            CheckIfEmpty();
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            
        }

    }
}