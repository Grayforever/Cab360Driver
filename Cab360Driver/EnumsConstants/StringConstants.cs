using Android;

namespace Cab360Driver.EnumsConstants
{
    public static class StringConstants
    {
        private static string gateway1 = "https://parseapi.back4app.com";
        private static string mapsNavigateBaseGateway = "google.navigation:q=";
        private static string mapsDirectionsBaseGateway = "https://maps.googleapis.com/maps/api/directions/";
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
    }
}