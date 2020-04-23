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
using Java.Net;

namespace Cab360Driver.Activities
{
    public class NetStateReceiver : BroadcastReceiver
    {
        public static bool online = true;
        public static string TAG = "Netconn";

        public override void OnReceive(Context context, Intent intent)
        {
            ConnectivityManager manager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo ni = manager.ActiveNetworkInfo;
            if(ni == null ||ni.IsConnected != true)
            {
                if (online)
                    Toast.MakeText(context, "No network", ToastLength.Short).Show();
                online = false;
            }
            else
            {
                if (!online)
                    if(IsOnline2() == true)
                    {
                        Toast.MakeText(context, "true", ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(context, "false", ToastLength.Short).Show();
                    }
                        
                online = true;
            }
        }

        public static bool IsOnline2()
        {
            try
            {
                int timeOutMs = 1500;
                Socket socket = new Socket();
                SocketAddress socketAddress = new InetSocketAddress("8.8.8.8", 53);

                socket.Connect(socketAddress, timeOutMs);
                socket.Close();
                return true;
            }
            catch (InvalidOperationException ioe)
            {

                return false;
            }
        }
    }
}