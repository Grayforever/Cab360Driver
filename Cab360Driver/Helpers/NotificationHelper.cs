using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Java.Lang;

namespace Cab360Driver.Helpers
{
    public class NotificationHelper : Java.Lang.Object
    {

        public const string PRIMARY_CHANNEL = "Urgent";
        public const int NOTIFY_ID = 100;

        public void NotifyVersion26(Context context, Android.Content.Res.Resources res, Android.App.NotificationManager manager)
        {

            string channelName = "Secondary Channel";
            var importance = NotificationImportance.High;
            var channel = new NotificationChannel(PRIMARY_CHANNEL, channelName, importance);

            var path = Android.Net.Uri.Parse("android.resource://com.graylabs.cab360/" + Resource.Raw.alert);
            var audioattribute = new AudioAttributes.Builder()
                .SetContentType(AudioContentType.Sonification)
                .SetUsage(AudioUsageKind.Notification).Build();

            channel.EnableLights(true);
            channel.EnableLights(true);
            channel.SetSound(path, audioattribute);
            channel.LockscreenVisibility = NotificationVisibility.Public;

            manager.CreateNotificationChannel(channel);

            Intent intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.CancelCurrent);

            Notification.Builder builder = new Notification.Builder(context, PRIMARY_CHANNEL)
                .SetContentTitle("Uber Driver")
                .SetSmallIcon(Resource.Drawable.ic_location)
                .SetLargeIcon(BitmapFactory.DecodeResource(res, Resource.Mipmap.ic_launcher))
                .SetContentText("You have a new trip request")
                .SetChannelId(PRIMARY_CHANNEL)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            manager.Notify(NOTIFY_ID, builder.Build());

                
        }
         
        [Obsolete]
        public void NotifyOtherVersions(Context context, Android.Content.Res.Resources res, NotificationManager manager)
        {
            Intent intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.CancelCurrent);
            var path = Android.Net.Uri.Parse("android.resource://com.ufinix.uberdriver/" + Resource.Raw.alert);


            Notification.Builder builder = new Notification.Builder(context)
                .SetContentTitle("Uber Driver")
                .SetSmallIcon(Resource.Drawable.ic_location)
                .SetLargeIcon(BitmapFactory.DecodeResource(res, Resource.Mipmap.ic_launcher))
                .SetTicker("You have a new trip request")
                .SetChannelId(PRIMARY_CHANNEL)
                .SetSound(path)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            manager.Notify(NOTIFY_ID, builder.Build());

        }
    }
}
