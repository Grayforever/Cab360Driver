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
    public class ProfileEventListener : Java.Lang.Object, IValueEventListener
    {

        private readonly ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.MultiProcess);
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
                    Code = (snapshot.Child("invitecode") != null) ? snapshot.Child("invitecode").Value.ToString() : ""
                };

                SaveToSharedPreference(DriverPersonal);
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

        private void SaveToSharedPreference(Driver driverPersonal)
        {
            editor.PutString("fname", driverPersonal.Fname);
            editor.PutString("lname", driverPersonal.Lname);
            editor.PutString("phone", driverPersonal.Phone);
            editor.PutString("email", driverPersonal.Email);
            editor.PutString("city", driverPersonal.City);
            editor.PutString("invitecode", driverPersonal.Code);
            editor.Apply();
        }
    }
}
