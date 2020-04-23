using System;
using Android.App;
using Android.Content;
using Firebase.Database;
using Cab360Driver.Helpers;
using Cab360Driver.DataModels;
using CN.Pedant.SweetAlert;

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
            new SweetAlertDialog(Application.Context, SweetAlertDialog.ErrorType)
                    .SetTitleText("Oops...")
                    .SetContentText("Something went wrong. Please try again later.")
                    .SetCancelText("OK")
                    .Show();
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if(snapshot.Value != null)
            {
                DriverPersonal = new Driver
                {
                    Fname = (snapshot.Child("firstname") != null) ? snapshot.Child("firstname").Value.ToString() : "",
                    Lname = (snapshot.Child("lastname") != null) ? snapshot.Child("lastname").Value.ToString() : "",
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

            DatabaseReference driverRef = database.GetReference("Cab360Drivers/" + currentUser.Uid);
            driverRef.AddValueEventListener(this);

        }

        private void SaveToSharedPreference(Driver driverPersonal)
        {
            editor.PutString("firstname", driverPersonal.Fname);
            editor.PutString("lastname", driverPersonal.Lname);
            editor.PutString("phone", driverPersonal.Phone);
            editor.PutString("email", driverPersonal.Email);
            editor.PutString("city", driverPersonal.City);
            editor.PutString("invitecode", driverPersonal.Code);
            editor.Apply();
        }
    }
}
