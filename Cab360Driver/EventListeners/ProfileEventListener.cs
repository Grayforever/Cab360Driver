using System;
using Android.App;
using Android.Content;
using Firebase.Database;
using Cab360Driver.Helpers;
using Cab360Driver.DataModels;
using CN.Pedant.SweetAlert;
using Android.Util;

namespace Cab360Driver.EventListeners
{
    public sealed class ProfileEventListener : Java.Lang.Object, IValueEventListener
    {

        private readonly ISharedPreferences preferences = Application.Context.GetSharedPreferences("userInfo", FileCreationMode.Private);
        private ISharedPreferencesEditor editor;
        private Driver DriverPersonal;
        public event EventHandler UserNotFoundEvent;
        public event EventHandler UserFoundEvent;

        public void OnCancelled(DatabaseError error)
        {
            Log.Error("profilevalueeventlistener", error.Message);
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if(snapshot.Exists())
            {
                DriverPersonal = new Driver
                {
                    Fname = (snapshot.Child("fname") != null) ? snapshot.Child("fname").Value.ToString() : "",
                    Lname = (snapshot.Child("lname") != null) ? snapshot.Child("lname").Value.ToString() : "",
                    Email = (snapshot.Child("email") != null) ? snapshot.Child("email").Value.ToString() : "",
                    Phone = (snapshot.Child("phone") != null) ? snapshot.Child("phone").Value.ToString() : "",
                    City = (snapshot.Child("city") != null) ? snapshot.Child("city").Value.ToString() : "",
                    Code = (snapshot.Child("invitecode") != null) ? snapshot.Child("invitecode").Value.ToString() : "",
                    ImgUrl = (snapshot.Child("profile_img_url") != null) ? snapshot.Child("profile_img_url").Value.ToString() : "",
                };

                SaveToSharedPreference();
                UserFoundEvent?.Invoke(this, new EventArgs());
            }
            else
            {
                UserNotFoundEvent?.Invoke(this, new EventArgs());
            }
        }

        public void Create()
        {
            editor = preferences.Edit();
            FirebaseDatabase database = AppDataHelper.GetDatabase();

            var currentUser = AppDataHelper.GetCurrentUser();
            if(currentUser != null)
            {
                DatabaseReference driverRef = database.GetReference("Drivers/" + currentUser.Uid);
                driverRef.AddValueEventListener(this);
            }
            else
            {
                return;
            }

        }

        private void SaveToSharedPreference()
        {
            editor.PutString("fname", DriverPersonal.Fname);
            editor.PutString("lname", DriverPersonal.Lname);
            editor.PutString("phone", DriverPersonal.Phone);
            editor.PutString("email", DriverPersonal.Email);
            editor.PutString("city", DriverPersonal.City);
            editor.PutString("invitecode", DriverPersonal.Code);
            editor.PutString("img_url", DriverPersonal.ImgUrl);
            editor.Apply();
        }
    }
}
