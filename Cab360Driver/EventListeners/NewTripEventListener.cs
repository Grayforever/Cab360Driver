using System;
using Firebase.Database;
using Cab360Driver.Helpers;

namespace Cab360Driver.EventListeners
{
    public class NewTripEventListener : Java.Lang.Object, IValueEventListener
    {
        string mRideID;
        Android.Locations.Location mLastlocation;
        FirebaseDatabase database;
        DatabaseReference tripRef;

        //flag
        bool isAccepted;
        public NewTripEventListener(string ride_id, Android.Locations.Location lastlocation)
        {
            mRideID = ride_id;
            mLastlocation = lastlocation;
            database = AppDataHelper.GetDatabase();
        }

        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if(snapshot.Value != null)
            {
                if (!isAccepted)
                {
                    isAccepted = true;
                    Accept();
                }
            }
        }

        public void Create()
        {
            tripRef = database.GetReference("rideRequest/" + mRideID);
            tripRef.LimitToLast(1)
                .AddValueEventListener(this);
        }

        void Accept()
        {
            tripRef.Child("status").SetValue("accepted");
            tripRef.Child("driver_name").SetValue(AppDataHelper.GetFirstName());
            tripRef.Child("driver_phone").SetValue(AppDataHelper.GetPhone());
            tripRef.Child("driver_location").Child("latitude").SetValue(mLastlocation.Latitude);
            tripRef.Child("driver_location").Child("longitude").SetValue(mLastlocation.Longitude);
            tripRef.Child("driver_id").SetValue(AppDataHelper.GetCurrentUser().Uid);
        }

        public void UpdateLocation(Android.Locations.Location lastlocation)
        {
            mLastlocation = lastlocation;
            tripRef.Child("driver_location").Child("latitude").SetValue(mLastlocation.Latitude);
            tripRef.Child("driver_location").Child("longitude").SetValue(mLastlocation.Longitude);

        }

        public void UpdateStatus(string status)
        {
            tripRef.Child("status").SetValue(status);
        }

        public void EndTrip (double fares)
        {

            //Update: Calls the garbage collector to release instances existing in memory. This hanles error: Invalid Instance. 
            GC.Collect();


            if(tripRef != null)
            {
                tripRef.Child("fares").SetValue(fares);
                tripRef.Child("status").SetValue("ended");
                tripRef.RemoveEventListener(this);
                tripRef = null;

            }
        }
    }
}
