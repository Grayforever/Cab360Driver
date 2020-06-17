using Android.App;
using Android.Content;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

namespace Cab360Driver.Helpers
{
    public static class AppDataHelper
    {
        private static readonly ISharedPreferences profilePref = Application.Context.GetSharedPreferences("driverInfo", FileCreationMode.Private);
        private static readonly ISharedPreferences earningsPref = Application.Context.GetSharedPreferences("earningsInfo", FileCreationMode.Private);
        private static readonly ISharedPreferences ratingsPref = Application.Context.GetSharedPreferences("ratingsInfo", FileCreationMode.Private);
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
            return FireDb.GetReference("Drivers");
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

        public static string Firstname => profilePref?.GetString("fname", "");

        public static string Lastname => profilePref?.GetString("lname", "");

        public static string Email => profilePref?.GetString("email", "");

        public static string Phone => profilePref?.GetString("phone", "");

        public static string City => profilePref?.GetString("city", "");

        public static string Fullname => $"{Firstname} {Lastname}";

        public static string CarDetails => profilePref?.GetString("carDetails", "");

        public static string TotRides => earningsPref?.GetString("totRides", "");

        public static string TotEarnings => earningsPref?.GetString("totEarnings", "");


    }
}