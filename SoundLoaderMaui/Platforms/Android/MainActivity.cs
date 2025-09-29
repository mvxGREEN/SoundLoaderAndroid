using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using UraniumUI.Material.Controls;
using static SoundLoaderMaui.MainPage;

namespace SoundLoaderMaui
{
    [Activity(Theme = "@style/MainTheme.NoActionBar", MainLauncher = true, Exported = true, LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(new[] { Intent.ActionSend },
          Categories = new[] {
              Intent.CategoryDefault
          },
          DataMimeType = "*/*")]
    [MetaData(name: "com.google.android.play.billingclient.version", Value = "7.1.1")]
    public class MainActivity : MauiAppCompatActivity
    {
        private static string Tag = nameof(MainActivity);
        public static FinishReceiver MFinishReceiver = new FinishReceiver();

        public static MainActivity ActivityCurrent { get; set; }
        public MainActivity()
        {
            ActivityCurrent = this;
        }

        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            Console.WriteLine($"{Tag}: OnCreate");

            //EdgeToEdge.Enable(this);
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            // Fixes "strict-mode" error when fetching webpage... idek..
            StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().PermitAll().Build();
            StrictMode.SetThreadPolicy(policy);

            /*// log ANRs
            StrictMode.SetVmPolicy(new StrictMode.VmPolicy.Builder()
                           .DetectAll()
                           .PenaltyLog()
                           //.PenaltyDeath()
                           .Build());
            */

            AskPermissions();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            Console.WriteLine($"{Tag}: OnNewIntent");

            CheckForIntent(intent);

        }

        public async Task CheckForIntent()
        {
            CheckForIntent(this.Intent);
        }

        public async Task CheckForIntent(Intent intent)
        {
            if (intent != null)
            {
                var data = intent.GetStringExtra(Intent.ExtraText);
                if (data != null)
                {
                    Console.WriteLine($"{Tag}: received data from intent: {data}");

                    MainPage mp = (MainPage)Shell.Current.CurrentPage;
                    await mp.ClearTextfield();
                    await mp.ShowEmptyUI();

                    Soundloader.MIsShared = true;

                    string SharedText = data.ToString();
                    TextField mTextField = (TextField)mp.FindByName("main_textfield");
                    if (mTextField != null)
                    {
                        mTextField.Text = SharedText;
                        mp.HandleInput(SharedText);
                    }
                    else
                    {
                        Console.WriteLine($"{Tag} null textfield!");
                    }
                }
            }
        }

        private void AskPermissions()
        {
            if ((int)Build.VERSION.SdkInt >= 33
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadMediaAudio) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(
                    MainActivity.ActivityCurrent, new string[] { Android.Manifest.Permission.ReadMediaAudio }, 101);

            }
            else if ((int)Build.VERSION.SdkInt < 33
                && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(
                MainActivity.ActivityCurrent, new string[] { Android.Manifest.Permission.ReadExternalStorage, Android.Manifest.Permission.WriteExternalStorage }, 101);
            }
        }

        // BROADCAST RECEIVERS
        [BroadcastReceiver(Enabled = true, Exported = false)]
        public class FinishReceiver : BroadcastReceiver
        {
            string Tag = "FinishReceiver";
            public override void OnReceive(Context context, Intent intent)
            {
                Console.WriteLine($"{Tag} OnReceive");

                // delete temp files
                Soundloader.DeleteTempFiles(new Java.IO.File(Soundloader.AbsPathDocsTemp));

                // update ui
                MainPage mp = ((MainPage)Shell.Current.CurrentPage);

                // stop service
                mp.Services.Stop();

                // unregister receiver
                try
                {
                    context.UnregisterReceiver(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{Tag} already unregistered");
                }

                if (Soundloader.MIsShared)
                {
                    Soundloader.ResetVars();
                    Soundloader.MIsShared = false;

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // increment successful runs
                        int runs = 1;
                        if (Preferences.Default.ContainsKey("SUCCESSFUL_RUNS"))
                        {
                            runs += Preferences.Default.Get("SUCCESSFUL_RUNS", 0);
                        }
                        successfulRuns = runs;

                        // set in prefs
                        Preferences.Default.Set("SUCCESSFUL_RUNS", runs);
                        Console.WriteLine($"{Tag} SUCCESSFUL_RUNS={runs}");

                        // show success message
                        mp.MMessageToast = $"Saved! In {Soundloader.AbsPathDocs}";
                        //AndHUD.Shared.ShowSuccess(MainActivity.ActivityCurrent, mp.MMessageToast, MaskType.Black, TimeSpan.FromMilliseconds(1600));

                        // clear views
                        await ((MainPage)Shell.Current.CurrentPage).ClearTextfield();
                        await ((MainPage)Shell.Current.CurrentPage).ShowEmptyUI();
                        await Task.Delay(400);
                        // finish activity
                        Platform.CurrentActivity.FinishAfterTransition();
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        mp.ShowFinishUI();
                    });
                }

            }
        }

    }
}
