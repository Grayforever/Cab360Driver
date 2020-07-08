using Android.OS;
using Android.Util;
using Android.Views;
using Cab360Driver.Activities;
using Cab360Driver.Helpers;
using Firebase;
using Firebase.Auth;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using System;

namespace Cab360Driver.Fragments
{
    public class ForgotPassFragment : AndroidX.Fragment.App.DialogFragment
    {
        private TextInputLayout emailEditText;

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
            emailEditText = view.FindViewById<TextInputLayout>(Resource.Id.forgot_email_et);
            var nextBtn = view.FindViewById<MaterialButton>(Resource.Id.btn_fgt_email_snd);
            emailEditText.EditText.TextChanged += (s1, e1) =>
            {
                nextBtn.Enabled = Patterns.EmailAddress.Matcher(emailEditText.EditText.Text).Matches();
            };

            nextBtn.Click += (s1, e1) =>
            {
                SendEmail();
            };
        }

        private void SendEmail()
        {
            OnboardingActivity.ShowProgressDialog();
            AppDataHelper.GetFirebaseAuth().SendPasswordResetEmail(emailEditText.EditText.Text)
                .AddOnCompleteListener(new OnCompleteListener(t =>
                {
                    if (!t.IsSuccessful)
                    {
                        try
                        {
                            OnboardingActivity.CloseProgressDialog();
                            throw t.Exception;
                        }
                        catch (FirebaseAuthEmailException)
                        {
                            OnboardingActivity.ShowErrorDialog("The email address provided is not asscociated with any Cab360 account. " +
                                "Please enter a valid Cab360 account related email");
                        }
                        catch (FirebaseAuthInvalidCredentialsException)
                        {
                            OnboardingActivity.ShowErrorDialog("The email provided is invalid");
                        }
                        catch (FirebaseNetworkException)
                        {
                            OnboardingActivity.ShowNoNetDialog(false);
                        }
                        catch (Exception e)
                        {
                            OnboardingActivity.ShowErrorDialog(e.Message);
                        }
                    }
                    else
                    {
                        OnboardingActivity.CloseProgressDialog();

                        EmailSentFragment emailBtmSht = new EmailSentFragment();
                        AndroidX.Fragment.App.FragmentTransaction ft = ChildFragmentManager.BeginTransaction();
                        ft.Add(emailBtmSht, "camera_intro");
                        ft.CommitAllowingStateLoss();
                        emailBtmSht.onResendClick += EmailBtmSht_onResendClick;
                        emailBtmSht.onOkClick += EmailBtmSht_onOkClick;
                    }

                }));
        }

        private void EmailBtmSht_onOkClick(object sender, EventArgs e)
        {
            DismissAllowingStateLoss();
        }

        private void EmailBtmSht_onResendClick(object sender, EventArgs e)
        {
            SendEmail();
        }
    }
}