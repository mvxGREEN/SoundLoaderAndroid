using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace SoundLoaderMaui.Platforms.Android
{
    [Service]
    public class DownloadService : Service, IServiceDownload
    {
        private static string Tag = "DownloadService";

        public static DownloadReceiver downloadReceiver;

        public const int NOTIFICATION_ID = 3599;
        const string channelId = "soundloader_channel";
        const string channelName = "SoundLoader";
        const string channelDescription = "SoundLoader's channel for notifications.";
        const string notificationTitle = "Downloading…";

        int max_progress = 100;
        int progress = 0;

        bool channelInitialized = false;
        int messageId = 0;
        int pendingIntentId = 0;

        PendingIntent pendingIntent;

        public List<string> inputs = new List<string>();

        public override IBinder OnBind(Intent intent)
        {
            Console.WriteLine($"{Tag} OnBind");

            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Console.WriteLine($"{Tag} OnStartCommand");

            if (intent.Action == "START_SERVICE")
            {
                RegisterDownloadReceiver();

                RegisterNotification();

                Task.Run(async () =>
                {
                    await Soundloader.DownloadM3u();
                });
                
            }
            else if (intent.Action == "STOP_SERVICE")
            {
                StopForeground(true);
                StopSelfResult(startId);
            }

            return StartCommandResult.NotSticky;
        }

        //Start and Stop Intents, set the actions for the MainActivity to get the state of the foreground service
        //Setting one action to start and one action to stop the foreground service
        public void Start()
        {
            Console.WriteLine($"{Tag} Start()");
            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(DownloadService));
            startService.SetAction("START_SERVICE");
            MainActivity.ActivityCurrent.StartService(startService);
        }

        public void Stop()
        {
            Console.WriteLine($"{Tag} Stop()");
            Intent stopIntent = new Intent(MainActivity.ActivityCurrent, Class);
            stopIntent.SetAction("STOP_SERVICE");
            MainActivity.ActivityCurrent.StartService(stopIntent);
        }

        private void RegisterNotification()
        {
            Console.WriteLine($"{Tag} RegisterNotification");

            Intent intent = new Intent(MainActivity.ActivityCurrent, typeof(MainActivity));
            //intent.PutExtra(TitleKey, title);
            //intent.PutExtra(MessageKey, message);
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;

            pendingIntent = PendingIntent.GetActivity(MainActivity.ActivityCurrent, pendingIntentId++, intent, pendingIntentFlags);

            NotificationChannel channel = new NotificationChannel(channelId, channelName, NotificationImportance.Max);
            NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);
            Notification notification = new Notification.Builder(this, channelId)
               .SetContentTitle(notificationTitle)
               //.SetSmallIcon(Resource.Drawable.downloader_raw)
               .SetProgress(max_progress, progress, false)
               .SetOngoing(true)
               .SetContentIntent(pendingIntent)
               .Build();

            manager.Notify(NOTIFICATION_ID, notification);
            //StartForeground(NOTIFICATION_ID, notification);
        }

        private void RegisterDownloadReceiver()
        {
            Console.WriteLine($"{Tag} RegisterDownloadReceiver");
            downloadReceiver = new DownloadReceiver();
            if ((int)Build.VERSION.SdkInt >= 33)
            {
                RegisterReceiver(downloadReceiver, new IntentFilter(DownloadManager.ActionDownloadComplete), ReceiverFlags.Exported);
            }
            else
            {
                RegisterReceiver(downloadReceiver, new IntentFilter(DownloadManager.ActionDownloadComplete));
            }
        }

        public void UpdateNotification(int progress, int max_progress)
        {
            Console.WriteLine($"{Tag} UpdateNotification");

            this.progress = progress;
            this.max_progress = max_progress;

            Intent intent = new Intent(MainActivity.ActivityCurrent, typeof(MainActivity));
            //intent.PutExtra(TitleKey, title);
            //intent.PutExtra(MessageKey, message);
            //intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;

            pendingIntent = PendingIntent.GetActivity(MainActivity.ActivityCurrent, pendingIntentId++, intent, pendingIntentFlags);

            var notification = GetNotification(pendingIntent);

            NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(NOTIFICATION_ID, notification);
        }

        Notification GetNotification(PendingIntent pIntent)
        {
            Console.WriteLine($"{Tag} GetNotification");

            if (pendingIntent == null)
            {
                Intent i = new Intent(MainActivity.ActivityCurrent, typeof(MainActivity));
                //intent.PutExtra(TitleKey, title);
                //intent.PutExtra(MessageKey, message);
                //intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

                var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S
                    ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                    : PendingIntentFlags.UpdateCurrent;
                pendingIntent = PendingIntent.GetActivity(MainActivity.ActivityCurrent, pendingIntentId, i, pendingIntentFlags);
            }
            return new Notification.Builder(this, channelId)
                    .SetContentTitle(notificationTitle)
               //.SetSmallIcon(Resource.Drawable.downloader_raw)
               .SetProgress(max_progress, progress, false)
               .SetOngoing(true)
               .SetContentIntent(pendingIntent)
               .Build();
        }
    }
}
