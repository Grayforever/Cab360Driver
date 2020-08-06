using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Cab360Driver.Activities;
using Cab360Driver.Adapters;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Cab360Driver.Utils;
using CN.Pedant.SweetAlert;
using Firebase;
using Firebase.Auth;
using Google.Android.Material.AppBar;
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
        private string fname, lname, email, phone, code, city;
        private SweetAlertDialog infoAlertDialog;
        public event EventHandler OnEmailExistsListener;

        public event EventHandler<SignUpSuccessArgs> SignUpSuccess;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.driver_signup_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            SubmitBtn = view.FindViewById<MaterialButton>(Resource.Id.drv_signup_sbmtbtn);
            FnameText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_fname_et);
            LnameText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_lname_et);
            EmailText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_email_et);
            PhoneText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_phone_et);
            CodeText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_code_et);
            PassText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signup_pass_et);
            CityText = view.FindViewById<AppCompatAutoCompleteTextView>(Resource.Id.autocity_et);
            var regAppBar = view.FindViewById<AppBarLayout>(Resource.Id.signup_appbar);

            var toolbar = regAppBar.FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.main_toolbar);
            toolbar.Title = "Register";
            toolbar.InflateMenu(Resource.Menu.help_menu);
            toolbar.MenuItemClick += Toolbar_MenuItemClick;

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
                //open progress
                OnboardingActivity.ShowProgressDialog();
                fname = FnameText.EditText.Text;
                lname = LnameText.EditText.Text;
                email = EmailText.EditText.Text;
                phone = PhoneText.EditText.Text;
                city = CityText.Text;
                code = CodeText.EditText.Text;

                AppDataHelper.GetFirebaseAuth().CreateUserWithEmailAndPassword(email, PassText.EditText.Text)
                .AddOnCompleteListener(new OnCompleteListener(t =>
                {
                    if (!t.IsSuccessful)
                    {
                        try
                        {
                            OnboardingActivity.CloseProgressDialog();
                            throw t.Exception;
                        }
                        catch (FirebaseAuthInvalidCredentialsException)
                        {
                            OnboardingActivity.ShowErrorDialog("The email or password is not correct");
                        }
                        catch (FirebaseAuthUserCollisionException)
                        {
                            ShowEmailExistsDialog();
                        }
                        catch (FirebaseAuthInvalidUserException)
                        {
                            OnboardingActivity.ShowErrorDialog("User invalid");
                        }
                        catch (FirebaseNetworkException)
                        {
                            OnboardingActivity.ShowNoNetDialog(false);
                        }
                        catch (Exception)
                        {
                            OnboardingActivity.ShowErrorDialog("Your request could not be completed at this time");
                        }
                    }
                    else
                    {
                        SaveDriverToDb();
                    }
                    
                }));
            };
        }

        private void Toolbar_MenuItemClick(object sender, AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            Toast.MakeText(Context, e.Item.ItemId + "", ToastLength.Short).Show();
        }

        private void EditText_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            
            CheckIfEmpty();
        }

        private void SaveDriverToDb()
        {
            var DriverRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{ AppDataHelper.GetCurrentUser().Uid}");

            HashMap compliments = new HashMap();
            compliments.Put("cool_car", "0");
            compliments.Put("neat_and_tidy", "0");
            compliments.Put("expert_navigation", "0");
            compliments.Put("awesome_music", "0");
            compliments.Put("made_me_laugh", "0");

            HashMap driverMap = new HashMap();
            driverMap.Put("fname", fname);
            driverMap.Put("lname", lname);
            driverMap.Put("email", email);
            driverMap.Put("phone", phone);
            driverMap.Put("city", city);
            driverMap.Put("invitecode", code);
            driverMap.Put("ratings", "0");
            driverMap.Put("compliments", compliments);
            driverMap.Put("created_at", DateTime.UtcNow.ToString());

            

            DriverRef.SetValue(driverMap)
                .AddOnSuccessListener(new OnSuccessListener(r=> 
                {
                    DriverRef.Child(StringConstants.StageofRegistration).SetValue(RegistrationStage.Partnering.ToString())
                        .AddOnSuccessListener(new OnSuccessListener(r2=> 
                        {
                            OnboardingActivity.CloseProgressDialog();
                            SignUpSuccess.Invoke(this, new SignUpSuccessArgs { IsCompleted = true });
                        }))
                        .AddOnFailureListener(new OnFailureListener(e1=> 
                        {
                            OnboardingActivity.CloseProgressDialog();
                            Toast.MakeText(Activity, e1.Message, ToastLength.Short).Show(); 
                        }));
                }))
                .AddOnFailureListener(new OnFailureListener(e=> 
                {
                    OnboardingActivity.ShowErrorDialog("Something went wrong, please retry");
                
                }));
        }

        private void CheckIfEmpty()
        {
            SubmitBtn.Enabled = Patterns.EmailAddress.Matcher(EmailText.EditText.Text).Matches() 
                && FnameText.EditText.Text.Length >= 3 
                && LnameText.EditText.Text.Length >= 3 
                && CityText.Text.Length >= 2 
                && PhoneText.EditText.Text.Length >= 8 
                && PassText.EditText.Text.Length >= 8;
        }

        private void ShowEmailExistsDialog()
        {
            infoAlertDialog = new SweetAlertDialog(Activity, SweetAlertDialog.ErrorType);
            infoAlertDialog.SetCancelable(false);
            infoAlertDialog.SetTitleText("Sign up error");
            infoAlertDialog.SetContentText("The email account provided is associated with an existing account. Would you like to sign in instead?");
            infoAlertDialog.SetCancelText("No");
            infoAlertDialog.SetConfirmText("Yes");
            infoAlertDialog.SetConfirmClickListener(new SweetConfirmClick(sweet =>
            {
                ClearTextFields();
                OnEmailExistsListener?.Invoke(this, new EventArgs());
                infoAlertDialog.DismissWithAnimation();
                infoAlertDialog = null;
            }));
            infoAlertDialog.Show();
        }

        private void ClearTextFields()
        {
            FnameText.EditText.Text = "";
            LnameText.EditText.Text = "";
            CityText.Text = "";
            PhoneText.EditText.Text = "";
            PassText.EditText.Text = "";
            EmailText.EditText.Text = "";
            CodeText.EditText.Text = "";
        }

        public class SignUpSuccessArgs : EventArgs
        {
            public bool IsCompleted { get; set; }
        }

    }
}