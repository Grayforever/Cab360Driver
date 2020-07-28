using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cab360Driver.Activities;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Google.Android.Material.Button;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.TextField;
using System;

namespace Cab360Driver.Fragments
{
    public class DriverSignInFragment : AndroidX.Fragment.App.Fragment
    {
        public event EventHandler<RegUncompleteArgs> onRegUncompleteListener;
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userInfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;
        public class RegUncompleteArgs : EventArgs
        {
            public string StageReached { get; set; }
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.driver_signin_layout, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var emailText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signin_email_et);
            var passText = view.FindViewById<TextInputLayout>(Resource.Id.drv_signin_pass_et);
            var forgotPassBtn = view.FindViewById<MaterialButton>(Resource.Id.btn_forgot_pass);
            forgotPassBtn.Click += (s1, e1) =>
            {
                ForgotPassFragment passFragment = new ForgotPassFragment();
                AndroidX.Fragment.App.FragmentTransaction ft = ChildFragmentManager.BeginTransaction();
                ft.Add(passFragment, "forgot_pass_fragment");
                ft.CommitAllowingStateLoss();
            };

            var continueFab = view.FindViewById<FloatingActionButton>(Resource.Id.fab1);
            continueFab.Click += (s2, e2) =>
            {
                bool enabled = !(!Patterns.EmailAddress.Matcher(emailText.EditText.Text).Matches() || passText.EditText.Text.Length < 8);
                switch (enabled)
                {
                    case false:
                        Toast.MakeText(Activity, "A valid email and password is required", ToastLength.Short).Show();
                        break;
                    default:
                        {
                            //showProgress
                            OnboardingActivity.ShowProgressDialog();
                            AppDataHelper.GetFirebaseAuth().SignInWithEmailAndPassword(emailText.EditText.Text, passText.EditText.Text)
                                .AddOnCompleteListener(new OnCompleteListener(t =>
                                {
                                    if (t.IsSuccessful)
                                    {
                                        var dataRef = AppDataHelper.GetDatabase().GetReference("Drivers/" + AppDataHelper.GetCurrentUser().Uid);
                                        dataRef.AddValueEventListener(new SingleValueListener(d =>
                                        {
                                            if (!d.Exists())
                                                return;

                                            var stage = (d.Child(StringConstants.StageofRegistration) != null) ? d.Child(StringConstants.StageofRegistration).Value.ToString() : "";
                                            if (stage != $"{RegistrationStage.RegistrationDone}")
                                            {
                                                editor = preferences.Edit();
                                                editor.PutString("firstRun", "reg");
                                                editor.Commit();
                                                OnboardingActivity.CloseProgressDialog();
                                                onRegUncompleteListener?.Invoke(this, new RegUncompleteArgs { StageReached = stage });
                                            }
                                            else
                                            {
                                                editor = preferences.Edit();
                                                editor.PutString("firstRun", "regd");
                                                editor.Commit();

                                                var intent = new Intent(Context, typeof(MainActivity));
                                                intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                                                StartActivity(intent);
                                            }

                                        }, e =>
                                        {
                                            try
                                            {
                                                OnboardingActivity.CloseProgressDialog();
                                                throw t.Exception;
                                            }
                                            catch (DatabaseException)
                                            {
                                                OnboardingActivity.ShowErrorDialog("Database exeption");
                                            }
                                            catch (FirebaseNetworkException)
                                            {
                                                OnboardingActivity.ShowNoNetDialog(false);
                                            }
                                            catch (Exception)
                                            {
                                                OnboardingActivity.ShowErrorDialog("Something went wrong, please retry");
                                            }
                                        }));
                                    }
                                    else
                                    {
                                        try
                                        {
                                            OnboardingActivity.CloseProgressDialog();
                                            throw t.Exception; 
                                        }
                                        catch(FirebaseAuthInvalidCredentialsException)
                                        {
                                            OnboardingActivity.ShowErrorDialog("Your email or password is incorrect. Please try again.");
                                        }
                                        catch(FirebaseAuthInvalidUserException)
                                        {
                                            OnboardingActivity.ShowErrorDialog("We can't find an account with this email address. Please try again or create a new account.");
                                        }
                                        catch(FirebaseNetworkException)
                                        {
                                            OnboardingActivity.ShowNoNetDialog(false);
                                        }
                                        catch(Exception)
                                        {
                                            OnboardingActivity.ShowErrorDialog("We couldn't sign you in at this time. Please retry");
                                        }
                                    }
                                    
                                }));
                                
                            break;
                        }
                }

            };
        }

    }
}