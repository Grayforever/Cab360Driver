using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Cab360Driver.Adapters;
using Cab360Driver.DataModels;
using Cab360Driver.Helpers;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Java.Lang;
using System;
using static Android.Views.View;

namespace Cab360Driver.Fragments
{
    public class DriverRegisterFragment : AndroidX.Fragment.App.Fragment, IOnKeyListener, ITextWatcher, IOnSuccessListener, IOnFailureListener
    {
        private TextInputLayout FnameText, LnameText, EmailText, PhoneText, PassText, CodeText;
        private AppCompatAutoCompleteTextView CityText;
        private MaterialButton SubmitBtn;
        private Driver DriverPersonal;
        private CoordinatorLayout driverSignupRoot;
        private string[] names = { "Accra", "Kumasi", "Taadi" };

        public class SignUpSuccessArgs : EventArgs
        {
            public Driver driver { get; set; }
        }

        public event EventHandler<SignUpSuccessArgs> SignUpSuccess;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            
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
            var auth = AppDataHelper.GetFirebaseAuth();

            FnameText.SetOnKeyListener(this);
            FnameText.EditText.AddTextChangedListener(this);

            LnameText.SetOnKeyListener(this);
            LnameText.EditText.AddTextChangedListener(this);

            PassText.EditText.AddTextChangedListener(this);
            PassText.SetOnKeyListener(this);

            EmailText.SetOnKeyListener(this);
            EmailText.EditText.AddTextChangedListener(this);

            PhoneText.SetOnKeyListener(this);
            PhoneText.EditText.AddTextChangedListener(this);

            var adapter = ArrayAdapterClass.CreateArrayAdapter(Activity, names);
            CityText.Adapter = adapter;
            CityText.SetOnKeyListener(this);
            CityText.AddTextChangedListener(this);

            SubmitBtn.Click += delegate
            {
                DriverPersonal = new Driver
                {
                    Fname = FnameText.EditText.Text,
                    Lname = LnameText.EditText.Text,
                    Phone = PhoneText.EditText.Text,
                    City = CityText.Text,
                    Code = CodeText.EditText.Text,
                    Email = EmailText.EditText.Text,
                    IsPartner = "true"
                };

                auth.CreateUserWithEmailAndPassword(DriverPersonal.Email, PassText.EditText.Text)
                    .AddOnSuccessListener(this)
                    .AddOnFailureListener(this);
            };
        }

        public void CheckIfEmpty()
        {
            var email = EmailText.EditText.Text;
            var fname = FnameText.EditText.Text;
            var lname = LnameText.EditText.Text;
            var city = CityText.Text;
            var code = CodeText.EditText.Text;
            var phone = PhoneText.EditText.Text;
            var pass = PassText.EditText.Text;

            SubmitBtn.Enabled = Android.Util.Patterns.EmailAddress.Matcher(email).Matches() && fname.Length >= 3 && 
                lname.Length >= 3 && city.Length >=2 && phone.Length >=8 && pass.Length >=8;
        }


        public void OnSuccess(Java.Lang.Object result)
        {
            SignUpSuccess?.Invoke(this, new SignUpSuccessArgs { driver = DriverPersonal });
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Android.Util.Log.Debug("sign uperror: ", e.Message);
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