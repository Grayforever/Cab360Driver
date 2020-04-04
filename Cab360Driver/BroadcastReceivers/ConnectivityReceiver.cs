using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Cab360Driver.BroadcastReceivers
{
    public class ConnectivityReceiver : BroadcastReceiver
    {
        public static IConnectivityReceiverListener connectivityReceiverListener;

        public ConnectivityReceiver()
        {
            
        }

        public static bool IsConnected(Context context)
        {
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo networkInfo = cm.ActiveNetworkInfo;
            return networkInfo != null && networkInfo.IsConnected;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo networkInfo = cm.ActiveNetworkInfo;
            bool isConnected = networkInfo != null && networkInfo.IsConnectedOrConnecting;
            if(connectivityReceiverListener != null)
            {
                connectivityReceiverListener.IOnNetworkConnectionChanged(isConnected);
            }
        }

        public interface IConnectivityReceiverListener
        {
            void IOnNetworkConnectionChanged(bool isConnected);
        }
    }
}