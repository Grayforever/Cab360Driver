using System;
using Firebase.Database;
using Java.Util;
using Cab360Driver.Helpers;
using AndroidX.AppCompat.App;
using Android.Content;

namespace Cab360Driver.EventListeners
{
    public class AvailablityListener : Java.Lang.Object, IValueEventListener
    {
        Context _context;
        DatabaseReference availablityRef;

        public AvailablityListener(Context context)
        {
            _context = context;
        }
        //assigneed
        public class RideAssignedIDEventArgs : EventArgs
        {
            public string RideId { get; set; }
        }
        public event EventHandler<RideAssignedIDEventArgs> RideAssigned;

        //cancelled
        public class RideCancelledArgs : EventArgs
        {
            public AlertDialog ShowWhy { get; set; }
        }
        public event EventHandler<RideCancelledArgs> RideCancelled;

        //timeout
        public class TimeoutMessageArgs : EventArgs
        {
            public string Message { get; set; }
        }
        public event EventHandler<TimeoutMessageArgs> RideTimedOut;
        
        public void Create (Android.Locations.Location myLocation)
        {
            availablityRef = AppDataHelper.GetAvailDrivRef().Child(AppDataHelper.GetCurrentUser().Uid);

            HashMap location = new HashMap();
            location.Put("latitude", myLocation.Latitude);
            location.Put("longitude", myLocation.Longitude);

            HashMap driverInfo = new HashMap();
            driverInfo.Put("location", location);
            driverInfo.Put("ride_id", "waiting");

            availablityRef.AddValueEventListener(this);
            availablityRef.SetValue(driverInfo);
        }

        public void RemoveListener()
        {
            availablityRef.RemoveValue();
            availablityRef.RemoveEventListener(this);
            availablityRef = null;
        }


        public void UpdateLocation(Android.Locations.Location mylocation)
        {
            if(availablityRef != null)
            {
                var locationref = availablityRef.Child("location");
                HashMap locationMap = new HashMap();
                locationMap.Put("latitude", mylocation.Latitude);
                locationMap.Put("longitude", mylocation.Longitude);
                locationref.SetValue(locationMap);
            }
        }

        public void ReActivate()
        {
            availablityRef.Child("ride_id").SetValue("waiting");
        }

        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (!snapshot.Exists())
                return;

            string ride_id = snapshot.Child("ride_id").Value.ToString();
            if (ride_id != "waiting" && ride_id != "timeout" && ride_id != "cancelled")
            {
                //Ride Assigned
                RideAssigned?.Invoke(this, new RideAssignedIDEventArgs { RideId = ride_id });
            }
            else if (ride_id == "timeout")
            {
                // Ride Timeout
                RideTimedOut?.Invoke(this, new TimeoutMessageArgs { Message = "Ride timeout" });
            }
            else if (ride_id == "cancelled")
            {
                //ride cancelled

                RideCancelled?.Invoke(this, new RideCancelledArgs { ShowWhy = null });
            }

        }
    }
}
