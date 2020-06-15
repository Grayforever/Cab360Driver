using Android.App;
using Android.Content;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

namespace Cab360Driver.Helpers
{
    public static class AppDataHelper
    {
        private static ISharedPreferences _pref1 = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        private static readonly ISharedPreferences _pref3 = Application.Context.GetSharedPreferences("earningsInfo", FileCreationMode.MultiProcess);
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
            return FireAuth?.CurrentUser;
        }

        private static string firstname = _pref1?.GetString("firstname", "");
        public static string GetFirstName()
        {
            return firstname;
        }

        private static string lastname = _pref1?.GetString("lastname", "");
        public static string GetLastName()
        {
            return lastname;
        }

        private static string email = _pref1?.GetString("email", "");
        public static string GetEmail()
        {
            return email;
        }

        private static string phone = _pref1?.GetString("phone", "");
        public static string GetPhone()
        {
            return phone;
        }

        private static string city = _pref1?.GetString("city", "");
        public static string GetCity()
        {
            return city;
        }

        private static string totEarnings = _pref3?.GetString("totalEarnings", "");
        public static string GetTotEarnings()
        {
            return totEarnings;
        }
    }
}