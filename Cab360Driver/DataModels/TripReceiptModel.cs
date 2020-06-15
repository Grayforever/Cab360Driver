namespace Cab360Driver.DataModels
{
    public class TripReceiptModel
    {
        public string Id { get; set; }
        public double Fare { get; set; }
        public double Distance { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public TripReceiptModel(string id, double fare, double distance, string from, string to)
        {
            Id = id;
            Fare = fare;
            Distance = distance;
            From = from;
            To = to;
        } 
    }
}