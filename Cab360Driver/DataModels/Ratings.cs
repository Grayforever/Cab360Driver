namespace Cab360Driver.DataModels
{
    public class Ratings
    {
        public int DriverID { get; set; }
        public int TotalRides { get; set; }
        public string RatingRange { get; set; }
        public double Star5Total { get; set; }
        public double Star4Total { get; set; }
        public double Star3Total { get; set; }
        public double Star2Total { get; set; }
        public double Star1Total { get; set; }
    }
}