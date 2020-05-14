using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;
using Cab360Driver.Helpers;
using Android.Util;
using Firebase.Database;
using Firebase.Auth;
using System;
using Cab360Driver.EnumsConstants;

namespace Cab360Driver.Activities
{
    [Activity(MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity, IValueEventListener
    {
        private ISharedPreferences preferences = Application.Context.GetSharedPreferences("appSession", FileCreationMode.MultiProcess);
        private ISharedPreferencesEditor editor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            editor = preferences.Edit();

        }

        protected override void OnResume()
        {
            base.OnResume();
            var currUser = AppDataHelper.GetCurrentUser();
            RouteToAppropriatePage(currUser);
        }

        private void RouteToAppropriatePage(FirebaseUser currUser)
        {
            if (currUser == null)
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
            fireDataRef.OrderByChild("stage_of_registration")
                .AddListenerForSingleValueEvent(this);
        }

        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            try
            {
                if (!snapshot.Exists())
                    return;

                var stage = snapshot.Child("stage_of_registration").Value.ToString();
                StartWhichActivity(stage);

            }
            catch (DatabaseException e)
            {
                Log.Debug("database exception: ", e.Message);
            }
        }

        private void StartWhichActivity(string stage)
        {
            Log.Debug("stage before exit: ", stage);
            if (!stage.Contains(RegistrationStage.Registration.ToString()))
            {
                var intent3 = new Intent(this, typeof(MainActivity));
                intent3.AddFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                StartActivity(intent3);
            }
            else
            {
                var intent1 = new Intent(this, typeof(OnboardingActivity));
                intent1.AddFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                StartActivity(intent1);
            }
            editor.PutString("stage_before_exit", stage);
            editor.Apply();
        }
    }
}
