using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Cab360Driver.DataModels;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Java.Lang;
using System;
using static Android.Views.View;

namespace Cab360Driver.Fragments
{
    public class DriverRegisterFragment : Android.Support.V4.App.Fragment, IOnKeyListener, ITextWatcher
    {
        private TextInputLayout FnameText, LnameText, EmailText, PhoneText, CityText, PassText, CodeText;
        private Button SubmitBtn;
        private Driver DriverPersonal;
        private FirebaseDatabase FireDatabase;
        private FirebaseAuth FireAuth;
        private TaskCompletionListeners TaskCompletionListener = new TaskCompletionListeners();


        public class StageOnePassEventArgs : EventArgs
        {
            public Driver driver { get; set; }
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
            return inflater.Inflate(Resource.Layout.driver_signup_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            GetControls(view);
        }

        private void GetControls(View view)
        {
            //button
            SubmitBtn = view.FindViewById<Button>(Resource.Id.drv_signup_sbmtbtn);
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

            CityText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_city_et);
            CityText.SetOnKeyListener(this);
            CityText.EditText.AddTextChangedListener(this);
        }

        private void CheckIfEmpty()
        {
            var email = EmailText.EditText.Text;
            var fname = FnameText.EditText.Text;
            var lname = LnameText.EditText.Text;
            var city = CityText.EditText.Text;
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
                City = CityText.EditText.Text,
                Code = CodeText.EditText.Text,
                Email = EmailText.EditText.Text,
                IsPartner = 0
                
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
                Toast.MakeText(Application.Context, "Error", ToastLength.Long).Show();
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