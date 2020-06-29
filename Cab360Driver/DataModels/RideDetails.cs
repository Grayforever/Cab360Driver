using System;
namespace Cab360Driver.DataModels
{
    public class RideDetails
    {
        public string PickupAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string RiderName { get; set; }
        public string RiderPhone { get; set; }
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public string RideId { get; set; }
        public string Distance { get; set; }
        public string Duration { get; set; }
        public string Fare { get; set; }
    }
}
