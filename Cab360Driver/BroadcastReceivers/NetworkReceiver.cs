using Android.Content;
using Android.Net;
using Android.Util;
using Cab360Driver.Activities;
using Java.Lang;

namespace Cab360Driver.BroadcastReceivers
{
    public class NetworkReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                if (IsOnline(context))
                {
                    OnboardingActivity.ShowNoNetDialog(true);
                }
                else
                {
                    OnboardingActivity.ShowNoNetDialog(false);
                }
            }
            catch (NullPointerException e)
            {
                Log.Error("labs", e.Message);
            }
        }

        private bool IsOnline(Context context)
        {
            try
            {
                ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                NetworkInfo netInfo = cm.ActiveNetworkInfo;
                //should check null because in airplane mode it will be null
                return netInfo != null && netInfo.IsConnected;
            }
            catch (NullPointerException e)
            {
                Log.Error("labs", e.Message);
                return false;
            }
        }
    }
}