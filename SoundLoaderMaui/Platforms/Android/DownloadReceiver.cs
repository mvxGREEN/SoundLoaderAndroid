using Android.App;
using Android.Content;
using MPowerKit.ProgressRing;
using static Soundloader;

namespace SoundLoaderMaui
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class DownloadReceiver : BroadcastReceiver
    {
        private static readonly string Tag = nameof(DownloadReceiver);

        public DownloadReceiver()
        {
            Console.WriteLine($"{Tag} new DownloadReceiver()");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            Console.WriteLine($"{Tag} OnReceive");
            MainPage mp = ((MainPage)Shell.Current.CurrentPage);
            string action = intent.Action;
            if (DownloadManager.ActionDownloadComplete.Equals(action))
            {
                Console.WriteLine($"{Tag} download complete.");
                if (MCountChunksFinal == 0)
                {
                    Console.WriteLine($"{Tag} m3u downloaded");

                    // update progress
                    ProgressRing pr = ((ProgressRing)mp.FindByName("progress_ring"));
                    double progress = MCountTracks / (double) MCountTracksFinal;
                    int percent = (int)(progress * 100.0);
                    pr.Progress = progress;
                    pr.IsIndeterminate = false;
                    mp.MMessageProgress = $"Downloading…\n{percent}%";

                    // extract chunk urls
                    MMp3Urls = ExtractMp3Urls(AbsPathDocsTemp + mM3uFileName + ".m3u");
                    MCountChunksFinal = mMp3Urls.Count;
                    Console.WriteLine($"{Tag} MCountChunksFinal={MCountChunksFinal}");

                    // download chunks
                    Task.Run(async () =>
                    {
                        for (int i = 0; i < MCountChunksFinal; i++)
                        {
                            Task.Delay(100).Wait();
                            await DownloadMp3(i);
                        }
                    });
                }
                else if (MCountChunks < MCountChunksFinal)
                {
                    ++MCountChunks;
                    Console.WriteLine($"{Tag} {MCountChunks}/{MCountChunksFinal} chunks downloaded!");

                    // calculate total progress
                    double progress = (((double)MCountChunks / (double)MCountChunksFinal) + MCountTracks) / (double)MCountTracksFinal;
                    int percent = (int)(progress * 100.0);
                    Console.WriteLine($"{Tag} total percent={percent} progress={progress}");

                    // update ui
                    ProgressRing pr = ((ProgressRing)mp.FindByName("progress_ring"));
                    pr.Progress = progress;
                    mp.MMessageProgress = mp.MMessageProgress = $"Downloading…\n{percent}%";

                    // if last chunk downloaded
                    if (MCountChunks == MCountChunksFinal)
                    {
                        // check for image
                        if (mp.MThumbnailUrl != "")
                        {
                            Task.Run(async () =>
                            {
                                // download image
                                Console.WriteLine($"{Tag} downloading image");
                                await DownloadImage();
                            });
                        }
                        else
                        {
                            // update progress
                            ((MainPage)Shell.Current.CurrentPage)
                                .MMessageProgress = "Finishing track…";
                            ((ProgressRing)mp.FindByName("progress_ring")).IsIndeterminate = true;

                            Task.Run(async () =>
                            {
                                await Task.Delay(200);
                                string filepath = await ConcatMp3();
                                await SetTags(filepath);
                                await ScanMp3();

                                DeleteTempFiles(new Java.IO.File(AbsPathDocsTemp));
                                ++MCountTracks;
                                if (MM3uUrls.Count > 0)
                                {
                                    // download next m3u
                                    MM3uUrl = MM3uUrls[0];
                                    MM3uUrls.Remove(MM3uUrls[0]);
                                    ResetVarsForNext();
                                    DownloadM3u();
                                }
                                else
                                {
                                    // or finish
                                    MainActivity.ActivityCurrent.SendBroadcast(new Intent("69"));
                                    Console.WriteLine($"{Tag} unregistering self");
                                    try
                                    {
                                        context.UnregisterReceiver(this);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine($"{Tag} already unregistered");
                                    }
                                }
                            });
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{Tag} image downloaded");

                    // update progress
                    ((MainPage)Shell.Current.CurrentPage)
                        .MMessageProgress = "Finishing track…";
                    ((ProgressRing)mp.FindByName("progress_ring")).IsIndeterminate = true;

                    Task.Run(async () =>
                    {
                        await Task.Delay(200);
                        string filepath = await ConcatMp3();
                        await SetTags(filepath);
                        await ScanMp3();

                        DeleteTempFiles(new Java.IO.File(AbsPathDocsTemp));
                        ++MCountTracks;
                        if (MM3uUrls.Count > 0)
                        {
                            // download next m3u
                            MM3uUrl = MM3uUrls[0];
                            MM3uUrls.Remove(MM3uUrls[0]);
                            ResetVarsForNext();
                            DownloadM3u();
                        }
                        else
                        {
                            // or finish
                            MainActivity.ActivityCurrent.SendBroadcast(new Intent("69"));
                            Console.WriteLine($"{Tag} unregistering self");
                            try
                            {
                                context.UnregisterReceiver(this);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{Tag} already unregistered");
                            }
                        }
                    });
                }
            }
        }
    }
}
