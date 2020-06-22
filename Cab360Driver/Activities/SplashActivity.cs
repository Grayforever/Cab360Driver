using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Cab360Driver.EnumsConstants;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using System;

namespace Cab360Driver.Activities
{
    [Activity(MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {
        private FirebaseDatabase fireDb;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            fireDb = AppDataHelper.GetDatabase();
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
                StartActivity(intent2);
                GC.Collect();
                Finish();
            }
            else
            {
                CheckStatus(currUser.Uid);
                
            }
        }

        private void CheckStatus(string uid)
        {
            var fireDataRef = fireDb.GetReference("Drivers").Child(uid);
            fireDataRef.OrderByChild("stage_of_registration").EqualTo($"{RegistrationStage.RegistrationDone}")
                .AddListenerForSingleValueEvent(new SingleValueListener(snapshot=> {
                    if (!snapshot.Exists())
                    {
                        var intent = new Intent(this, typeof(OnboardingActivity));
                        intent.PutExtra("stage", snapshot.Value.ToString());
                        StartActivity(intent);
                        Finish();
                    }
                    else
                    {
                        var intent3 = new Intent(this, typeof(MainActivity));
                        StartActivity(intent3);
                        Finish();
                    }
                }, error=> 
                {
                    Toast.MakeText(this, error.Message, ToastLength.Short).Show();
                }));
        }
    }
}
