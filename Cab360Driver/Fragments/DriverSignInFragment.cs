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
                passFragment.Show(Activity.SupportFragmentManager, "show forgot fragment");
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
                                        var fireDb = AppDataHelper.GetDatabase();
                                        var dataRef = fireDb.GetReference("Drivers/" + AppDataHelper.GetCurrentUser().Uid);
                                        dataRef.AddValueEventListener(new SingleValueListener(d =>
                                        {
                                            if (!d.Exists())
                                                return;

                                            var stage = (d.Child(StringConstants.StageofRegistration) != null) ? d.Child(StringConstants.StageofRegistration).Value.ToString() : "";
                                            if (stage != $"{RegistrationStage.RegistrationDone}")
                                            {
                                                OnboardingActivity.CloseProgressDialog();
                                                onRegUncompleteListener?.Invoke(this, new RegUncompleteArgs { StageReached = stage });
                                            }
                                            else
                                            {
                                                string fname, lname, email, phone, city;
                                                fname = (d.Child("fname") != null) ? d.Child("fname").Value.ToString() : "";
                                                lname = (d.Child("lname") != null) ? d.Child("lname").Value.ToString() : "";
                                                email = (d.Child("email") != null) ? d.Child("email").Value.ToString() : "";
                                                phone = (d.Child("phone") != null) ? d.Child("phone").Value.ToString() : "";
                                                city = (d.Child("city") != null) ? d.Child("city").Value.ToString() : "";

                                                ISharedPreferencesEditor editor = StringConstants.GetEditor();
                                                editor.PutString("fname", fname);
                                                editor.PutString("lname", lname);
                                                editor.PutString("email", email);
                                                editor.PutString("phone", phone);
                                                editor.PutString("city", city);
                                                editor.Apply();

                                                OnboardingActivity.CloseProgressDialog();
                                                var intent = new Intent(Activity, typeof(MainActivity));
                                                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
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
                                            OnboardingActivity.ShowErrorDialog("The email or password is not correct");
                                        }
                                        catch(FirebaseAuthUserCollisionException)
                                        {
                                            OnboardingActivity.ShowErrorDialog("The email provided is associated with an existing account");
                                        }
                                        catch(FirebaseAuthInvalidUserException)
                                        {
                                            OnboardingActivity.ShowErrorDialog("User account not found");
                                        }
                                        catch(FirebaseNetworkException)
                                        {
                                            OnboardingActivity.ShowNoNetDialog(false);
                                        }
                                        catch(Exception)
                                        {
                                            OnboardingActivity.ShowErrorDialog("We couldnt sign you in at this time. Please retry");
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