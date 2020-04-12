using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Cab360Driver.EventListeners;
using Cab360Driver.Helpers;
using Firebase.Auth;
using Firebase.Database;
using Java.Lang;

namespace Cab360Driver.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/AppTheme", ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class SplashActivity : AppCompatActivity, IValueEventListener
    {
        private FirebaseUser firebaseUser = AppDataHelper.GetCurrentUser();
        private ISharedPreferences preferences = Application.Context.GetSharedPreferences("appSession", FileCreationMode.Private);
        private ISharedPreferencesEditor editor;

        public void OnCancelled(DatabaseError error)
        {
            
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if(snapshot.Value != null)
            {
                var stage = snapshot.Child("stage_of_registration").Value.ToString();
                try
                {
                    StartWhichActivity(stage);
                }
                catch
                {

                }
            }
        }

        private void StartWhichActivity(string stage)
        {
            Toast.MakeText(this, stage, ToastLength.Long).Show();
            if(stage == "1" || stage == "2")
            {
                var intent1 = new Intent(this, typeof(OnboardingActivity));
                
                StartActivity(intent1);
                Finish();
            }
            else
            {
                var intent3 = new Intent(this, typeof(MainActivity));
                
                StartActivity(intent3);
                Finish();
            }
            editor.PutString("stage_before_exit", stage);
            editor.Apply();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            editor = preferences.Edit();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (firebaseUser == null)
            {
                var intent2 = new Intent(this, typeof(OnboardingActivity));
                StartActivity(intent2);
            }
            else
            {
                var uid = firebaseUser.Uid;
                CheckStatus(uid);
            }
            //StartActivity(typeof(MainActivity));
        }

        private void CheckStatus(string uid)
        {
            var fireDataRef = AppDataHelper.GetDatabase().GetReference("Cab360Drivers").Child(uid);
            fireDataRef.OrderByChild("stage_of_registration")
                .AddListenerForSingleValueEvent(this);

        }
    }
}
