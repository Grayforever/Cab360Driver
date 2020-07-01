using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Auth;

namespace Cab360Driver.Activities
{
    [Activity(MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RouteToAppropriatePage(AppDataHelper.GetCurrentUser());
        }

        private void RouteToAppropriatePage(FirebaseUser currUser)
        {
            if(currUser is null)
            {
                StartActivity(typeof(OnboardingActivity));
                Finish();
            }
            else
            {
                CheckStatus(currUser.Uid);
            }
        }

        private void CheckStatus(string uid)
        {
            var fireDataRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{uid}");
            fireDataRef.Child(StringConstants.StageofRegistration)
                .AddValueEventListener(new SingleValueListener(snapshot=> {
                    if (snapshot.Exists())
                    {
                        var stage = snapshot.Value.ToString();
                        if (stage == $"{RegistrationStage.RegistrationDone}")
                        {
                            StartActivity(typeof(MainActivity));
                            Finish();
                        }
                        else
                        {
                            var intentSub = new Intent(this, typeof(OnboardingActivity));
                            intentSub.PutExtra("stage", stage);
                            StartActivity(intentSub);
                            Finish();
                        }
                    }
                    else
                    {
                        StartActivity(typeof(OnboardingActivity));
                        Finish();
                    }
                }, error=> 
                {
                    Toast.MakeText(this, error.Message, ToastLength.Short).Show();
                }));
        }
    }
}
