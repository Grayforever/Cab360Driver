using Android;
using Android.App;
using Android.Content;
using System.Text.RegularExpressions;

namespace Cab360Driver.EnumsConstants
{
    public static class StringConstants
    {
        private static string gateway1 = "https://parseapi.back4app.com";
        private static string mapsNavigateBaseGateway = "google.navigation:q=";
        private static string mapsDirectionsBaseGateway = "https://maps.googleapis.com/maps/api/directions/";
        private static Regex regex = new Regex(@"^[a-zA-Z]{2}-\d{4}\-(\d{2}|[a-zA-Z])$");
        private static string[] permissionsGroup =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        public static string GetGateway()
        {
            return gateway1;
        }

        public static string GetNavigateBaseGateway()
        {
            return mapsNavigateBaseGateway;
        }

        public static string GetDirectionsBaseGateway()
        {
            return mapsDirectionsBaseGateway;
        }

        public static string[] GetLocationPermissiongroup()
        {
            return permissionsGroup;
        }

        public static string StageofRegistration => "stage_of_registration";

        public static bool IsCarNumMatch(string carNum)
        {
            return regex.IsMatch(carNum);
        }
    }
}