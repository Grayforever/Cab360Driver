using Android.App;
using Android.Content;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

namespace Cab360Driver.Helpers
{
    public class AppDataHelper
    {
        static ISharedPreferences pref = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        static ISharedPreferences prefs = Application.Context.GetSharedPreferences("appSession", FileCreationMode.Private);


        public static FirebaseDatabase GetDatabase()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseDatabase database;
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("taxiproject-185a4")
                    .SetApiKey("AIzaSyDHXqe3Yh9Nl3wsxFItOoz1IwKiBRP7fxk")
                    .SetDatabaseUrl("https://taxiproject-185a4.firebaseio.com")
                    .SetStorageBucket("taxiproject-185a4.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(Application.Context, options);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }
            return database;
        }

        public static FirebaseApp GetFirebaseApp()
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

        public static FirebaseAuth GetFirebaseAuth()
        {

            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseAuth mAuth;
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("taxiproject-185a4")
                    .SetApiKey("AIzaSyDHXqe3Yh9Nl3wsxFItOoz1IwKiBRP7fxk")
                    .SetDatabaseUrl("https://taxiproject-185a4.firebaseio.com")
                    .SetStorageBucket("taxiproject-185a4.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(Application.Context, options);
                mAuth = FirebaseAuth.Instance;
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
            }

            return mAuth;
        }

        public static FirebaseUser GetCurrentUser()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseAuth mAuth;
            FirebaseUser mUser;
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("taxiproject-185a4")
                    .SetApiKey("AIzaSyDHXqe3Yh9Nl3wsxFItOoz1IwKiBRP7fxk")
                    .SetDatabaseUrl("https://taxiproject-185a4.firebaseio.com")
                    .SetStorageBucket("taxiproject-185a4.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(Application.Context, options);
                mAuth = FirebaseAuth.Instance;
                mUser = mAuth.CurrentUser;
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
                mUser = mAuth.CurrentUser;
            }

            return mUser;
        }

        public static string GetFirstName()
        {
            return pref?.GetString("firstname", ""); ;
        }

        public static string GetLastName()
        {
            return pref?.GetString("lastname", "");
        }

        public static string GetEmail()
        {
            string email = pref?.GetString("email", "");
            return email;
        }

        public static string GetPhone()
        {
            return pref?.GetString("phone", "");
        }

        public static string GetCity()
        {
            return pref?.GetString("city", "");
        }


        public static string GetStage()
        {
            return prefs?.GetString("stage_before_exit", "");
        }
    }
}