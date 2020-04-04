using System;
using Firebase.Database;
using Cab360Driver.DataModels;
using Cab360Driver.Helpers;

namespace Cab360Driver.EventListeners
{
    public class RideDetailsListener : Java.Lang.Object, IValueEventListener
    {
      
        public class RideDetailsEventArgs : EventArgs
        {
            public RideDetails RideDetails { get; set; }
        }

        public event EventHandler<RideDetailsEventArgs> RideDetailsFound;
        public event EventHandler RideDetailsNotFound;

        public void OnCancelled(DatabaseError error)
        {
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if(snapshot.Value != null)
            {
                RideDetails rideDetails = new RideDetails();
                rideDetails.DestinationAddress = snapshot.Child("destination_address").Value.ToString();
                rideDetails.DestinationLat = double.Parse(snapshot.Child("destination").Child("latitude").Value.ToString());
                rideDetails.DestinationLng = double.Parse(snapshot.Child("destination").Child("longitude").Value.ToString());

                rideDetails.PickupAddress = snapshot.Child("pickup_address").Value.ToString();
                rideDetails.PickupLat = double.Parse(snapshot.Child("location").Child("latitude").Value.ToString());
                rideDetails.PickupLng = double.Parse(snapshot.Child("location").Child("longitude").Value.ToString());


                rideDetails.RideId = snapshot.Key;
                rideDetails.RiderName = snapshot.Child("rider_name").Value.ToString();
                rideDetails.RiderPhone = snapshot.Child("rider_phone").Value.ToString();
                RideDetailsFound?.Invoke(this, new RideDetailsEventArgs { RideDetails = rideDetails });
            }
            else
            {
                RideDetailsNotFound?.Invoke(this, new EventArgs());
            }
        }


        public void Create(string ride_id)
        {
            FirebaseDatabase database = AppDataHelper.GetDatabase();
            DatabaseReference rideDetailsRef = database.GetReference("rideRequest/" + ride_id);
            rideDetailsRef.AddListenerForSingleValueEvent(this);
        }
    }
}
