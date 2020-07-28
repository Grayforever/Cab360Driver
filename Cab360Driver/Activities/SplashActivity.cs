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
    [Activity(Theme ="@style/AppTheme", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userInfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            string firstRun = preferences.GetString("firstRun", "");

            if (firstRun != "" && firstRun != "reg")
            {
                StartActivity(typeof(MainActivity));
                Finish();
            }
            else
            {
                editor = preferences.Edit();
                editor.PutString("firstRun", "reg");
                editor.Commit();
                RouteToAppropriatePage(AppDataHelper.GetCurrentUser());
            }
        }

        private void RouteToAppropriatePage(FirebaseUser currUser)
        {
            if (currUser == null)
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
            var fireDataRef = AppDataHelper.GetDatabase().GetReference($"Drivers/{uid}/{StringConstants.StageofRegistration}");
            fireDataRef.AddValueEventListener(new SingleValueListener(snapshot => 
            {
                if (snapshot.Exists())
                {
                    var stage = snapshot.Value.ToString();

                    var intentSub = new Intent(this, typeof(OnboardingActivity));
                    intentSub.PutExtra("stage", stage);
                    StartActivity(intentSub);
                    Finish();
                }
                else
                {
                    StartActivity(typeof(OnboardingActivity));
                    Finish();
                }
            }, error =>
            {
                Toast.MakeText(this, error.Message, ToastLength.Short).Show();
            }));
        }
    }
}
