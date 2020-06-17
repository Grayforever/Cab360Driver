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
                var intent2 = new Intent(this, typeof(OnboardingActivity));
                intent2.AddFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                StartActivity(intent2);
            }
            else
            {
                CheckStatus(currUser.Uid);
                
            }
        }

        private void CheckStatus(string uid)
        {
            var fireDataRef = AppDataHelper.GetParentReference().Child(uid);
            fireDataRef.OrderByChild("stage_of_registration").EqualTo(RegistrationStage.RegistrationDone.ToString())
                .AddListenerForSingleValueEvent(new SingleValueListener(snapshot=> {
                    if (!snapshot.Exists())
                    {
                        var intent = new Intent(this, typeof(OnboardingActivity));
                        intent.AddFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                        intent.PutExtra("stage", snapshot.Value.ToString());
                        StartActivity(intent);

                    }
                    else
                    {
                        var intent3 = new Intent(this, typeof(MainActivity));
                        intent3.AddFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                        StartActivity(intent3);
                    }
                }, error=> 
                {
                    Toast.MakeText(this, error.Message, ToastLength.Short).Show();
                }));
        }
    }
}
