using Android.App;
using Android.Content;
using Cab360Driver.EnumsConstants;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;

namespace Cab360Driver.Helpers
{
    public static class AppDataHelper
    {
        private static ISharedPreferences pref = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        private static ISharedPreferences prefs = Application.Context.GetSharedPreferences("appSession", FileCreationMode.Private);
        private static FirebaseAuth FireAuth;
        private static FirebaseDatabase FireDb;

        private static FirebaseApp FireApp 
        {
            get
            {
                var app = FirebaseApp.InitializeApp(Application.Context);
                if (app == null)
                {
                    var options = new FirebaseOptions.Builder()
                        .SetApplicationId("taxiproject-185a4")
                        .SetApiKey("AIzaSyDHXqe3Yh9Nl3wsxFItOoz1IwKiBRP7fxk")
                        .SetDatabaseUrl("https://taxiproject-185a4.firebaseio.com")
                        .SetStorageBucket("taxiproject-185a4.appspot.com")
                        .Build();
                    app = FirebaseApp.InitializeApp(Application.Context, options);
                }
                return app;
            }
        }

        private static FirebaseApp GetFireApp()
        {
            return FireApp;
        }
        
        public static FirebaseDatabase GetDatabase()
        {
            GetFireApp();
            FireDb = FirebaseDatabase.GetInstance(FireApp);
            return FireDb;
        }

        public static DatabaseReference GetParentReference()
        {
            GetDatabase();
            return FireDb.GetReference("Cab360Drivers");
        }

        public static DatabaseReference GetAvailDrivRef()
        {
            GetDatabase();
            return FireDb.GetReference("driversAvailable");
        }

        public static FirebaseAuth GetFirebaseAuth()
        {
            GetFireApp();
            FireAuth = FirebaseAuth.GetInstance(FireApp);
            return FireAuth;
        }

        public static FirebaseUser GetCurrentUser()
        {
            GetFirebaseAuth();
            var mUser = FireAuth.CurrentUser;
            return mUser;
        }

        private static string firstname = pref?.GetString("firstname", "");
        public static string GetFirstName()
        {
            return firstname;
        }

        private static string lastname = pref?.GetString("lastname", "");
        public static string GetLastName()
        {
            return lastname;
        }

        private static string email = pref?.GetString("email", "");
        public static string GetEmail()
        {
            
            return email;
        }

        private static string phone = pref?.GetString("phone", "");
        public static string GetPhone()
        {
            return phone;
        }

        private static string city = pref?.GetString("city", "");
        public static string GetCity()
        {
            return city;
        }

        private static string stage = prefs?.GetString("stage_before_exit", "");
        public static string GetStage()
        {
            return stage;
        }
    }
}