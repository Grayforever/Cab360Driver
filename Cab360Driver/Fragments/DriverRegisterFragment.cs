using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.DataModels;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.Util;
using System;
using static Android.Views.View;

namespace Cab360Driver.Fragments
{
    public class DriverRegisterFragment : AndroidX.Fragment.App.Fragment, IOnKeyListener, IOnSuccessListener, IOnFailureListener
    {
        private TextInputLayout FnameText, LnameText, EmailText, PhoneText, PassText, CodeText;
        private AppCompatAutoCompleteTextView CityText;
        private MaterialButton SubmitBtn;
        private CoordinatorLayout driverSignupRoot;
        private string[] names = { "Accra", "Kumasi", "Taadi" };
        private TaskCompletionListeners DriverProfileListener = new TaskCompletionListeners();
        private TaskCompletionListeners TaskCompletionListener = new TaskCompletionListeners();
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
            driverSignupRoot = view.FindViewById<CoordinatorLayout>(Resource.Id.drv_signup_root);
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
            FnameText.SetOnKeyListener(this);
            FnameText.EditText.AfterTextChanged += EditText_AfterTextChanged;

            LnameText.SetOnKeyListener(this);
            LnameText.EditText.AfterTextChanged += EditText_AfterTextChanged;

            PassText.EditText.AfterTextChanged += EditText_AfterTextChanged;
            PassText.SetOnKeyListener(this);

            EmailText.SetOnKeyListener(this);
            EmailText.EditText.AfterTextChanged += EditText_AfterTextChanged;

            PhoneText.SetOnKeyListener(this);
            PhoneText.EditText.AfterTextChanged += EditText_AfterTextChanged;

            var adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, names);
            CityText.Adapter = adapter;
            CityText.SetOnKeyListener(this);
            CityText.AfterTextChanged += EditText_AfterTextChanged;

            SubmitBtn.Click += (s1, e1) =>
            {
                FireAuth = AppDataHelper.GetFirebaseAuth();
                FireAuth.CreateUserWithEmailAndPassword(EmailText.EditText.Text, PassText.EditText.Text)
                    .AddOnSuccessListener(this)
                    .AddOnFailureListener(this);
            };
        }

        private void EditText_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            CheckIfEmpty();
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            var driverPersonal = new Driver
            {
                Fname = FnameText.EditText.Text,
                Lname = LnameText.EditText.Text,
                Phone = PhoneText.EditText.Text,
                City = CityText.Text,
                Code = CodeText.EditText.Text,
                Email = EmailText.EditText.Text,
                IsPartner = "true"
            };
            HashDriver(driverPersonal);
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Log.Debug("sign uperror: ", e.Message);
        }

        private void HashDriver(Driver driverPersonal)
        {
            HashMap Cab360Drivers = new HashMap();
            Cab360Drivers.Put("firstname", driverPersonal.Fname);
            Cab360Drivers.Put("lastname", driverPersonal.Lname);
            Cab360Drivers.Put("email", driverPersonal.Email);
            Cab360Drivers.Put("phone", driverPersonal.Phone);
            Cab360Drivers.Put("city", driverPersonal.City);
            Cab360Drivers.Put("invitecode", driverPersonal.Code);
            Cab360Drivers.Put("created_at", DateTime.UtcNow.ToString());
            Cab360Drivers.Put("isPartner", driverPersonal.IsPartner);
            Cab360Drivers.Put("stage_of_registration", RegistrationStage.Registration.ToString());
            SaveDriverToDb(Cab360Drivers);
        }

        private void SaveDriverToDb(HashMap cab360Drivers)
        {
            driverRef = AppDataHelper.GetParentReference().Child(FireAuth.CurrentUser.Uid);
            driverRef.SetValue(cab360Drivers)
                .AddOnSuccessListener(DriverProfileListener)
                .AddOnFailureListener(DriverProfileListener);
            DriverProfileListener.Successful += DriverProfileListener_Successful;
            DriverProfileListener.Failure += DriverProfileListener_Failure;
        }

        private void DriverProfileListener_Failure(object sender, EventArgs e)
        {

        }

        private void DriverProfileListener_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            driverRef.Child("stage_of_registration").SetValue(RegistrationStage.Partnering.ToString())
                .AddOnSuccessListener(TaskCompletionListener)
                .AddOnFailureListener(TaskCompletionListener);
            TaskCompletionListener.Successful += TaskCompletionListener_Successful;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure;
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            
        }

        private void TaskCompletionListener_Successful(object sender, TaskCompletionListeners.ResultArgs e)
        {
            SignUpSuccess.Invoke(this, new SignUpSuccessArgs { IsCompleted = true });
        }

        private void CheckIfEmpty()
        {
            SubmitBtn.Enabled = Patterns.EmailAddress.Matcher(EmailText.EditText.Text).Matches() && FnameText.EditText.Text.Length >= 3 &&
                LnameText.EditText.Text.Length >= 3 && CityText.Text.Length >= 2 && PhoneText.EditText.Text.Length >= 8 && PassText.EditText.Text.Length >= 8;
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

        public class SignUpSuccessArgs : EventArgs
        {
            public bool IsCompleted { get; set; }
        }

    }
}