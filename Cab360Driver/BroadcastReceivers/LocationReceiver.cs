using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Apache.Http.Conn;

namespace Cab360Driver.BroadcastReceivers
{
    public class LocationReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (IsOnline(context))
            {

            }
            else
            {

            }
        }

        private bool IsOnline(Context context)
        {
            LocationManager locationManager = (LocationManager)context.GetSystemService(Context.LocationService);
            try
            {
                return (locationManager != null && locationManager.IsProviderEnabled(LocationManager.GpsProvider));
            }
            catch
            {
                return false;
            }
            
        }
    }
}