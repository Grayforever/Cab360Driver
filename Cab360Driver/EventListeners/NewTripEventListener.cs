using System;
using Firebase.Database;
using Cab360Driver.Helpers;
using Cab360Driver.EnumsConstants;
using Android.Locations;

namespace Cab360Driver.EventListeners
{
    public class NewTripEventListener : Java.Lang.Object, IValueEventListener
    {
        private string mRideID;
        private Location mLastlocation;
        private DatabaseReference tripRef;
        private RideStatusEnum statusEnum;

        //flag
        bool isAccepted;
        public NewTripEventListener(string ride_id, Location lastlocation)
        {
            mRideID = ride_id;
            mLastlocation = lastlocation;
        }

        

        public void Create()
        {
            tripRef = AppDataHelper.GetDatabase().GetReference("rideRequest/" + mRideID);
            tripRef.LimitToLast(1)
                .AddValueEventListener(this);
        }

        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Value != null)
            {
                if (!isAccepted)
                {
                    isAccepted = true;
                    Accept();
                }
            }
        }

        void Accept()
        {
            statusEnum = RideStatusEnum.Accepted;
            tripRef.Child("status").SetValue($"{statusEnum}");
            tripRef.Child("driver_name").SetValue(AppDataHelper.GetFirstName());
            tripRef.Child("driver_phone").SetValue(AppDataHelper.GetPhone());
            tripRef.Child("driver_location").Child("latitude").SetValue(mLastlocation.Latitude);
            tripRef.Child("driver_location").Child("longitude").SetValue(mLastlocation.Longitude);
            tripRef.Child("driver_id").SetValue(AppDataHelper.GetCurrentUser().Uid);
        }

        public void UpdateLocation(Location lastlocation)
        {
            mLastlocation = lastlocation;
            tripRef.Child("driver_location").Child("latitude").SetValue(mLastlocation.Latitude);
            tripRef.Child("driver_location").Child("longitude").SetValue(mLastlocation.Longitude);
        }

        public void UpdateStatus(RideStatusEnum status)
        {
            tripRef.Child("status").SetValue($"{status}");
        }

        public void EndTrip (double fares)
        {
            //Update: Calls the garbage collector to release instances existing in memory. This hanles error: Invalid Instance. 
            GC.Collect();
            if (tripRef != null)
            {
                statusEnum = RideStatusEnum.Ended;
                tripRef.Child("fares").SetValue(fares);
                tripRef.Child("status").SetValue($"{statusEnum}");
                tripRef.RemoveEventListener(this);
                tripRef = null;

            }
        }
    }
}
