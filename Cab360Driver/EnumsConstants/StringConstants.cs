using Android;
using Android.App;

namespace Cab360Driver.EnumsConstants
{
    public static class StringConstants
    {
        private static readonly string gateway1 = "https://parseapi.back4app.com";
        private static readonly string mapsNavigateBaseGateway = "google.navigation:q=";
        private static readonly string mapsDirectionsBaseGateway = "https://maps.googleapis.com/maps/api/directions/";
        private static readonly string mapKey = Application.Context.GetString(Resource.String.google_api_key);
        private static readonly string databaseUrl = Application.Context.GetString(Resource.String.databaseUrl);
        private static readonly string[] permissionsGroup =
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

        public static string GetGoogleApiKey()
        {
            return mapKey;
        }

        public static string[] GetLocationPermissiongroup()
        {
            return permissionsGroup;
        }

        public static string GetDatabaseUrl()
        {
            return databaseUrl;
        }
    }
}