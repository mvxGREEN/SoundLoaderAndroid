using SoundLoaderMaui;
using TagLib;
using Console = System.Console;
using File = System.IO.File;
using IPicture = TagLib.IPicture;

#if ANDROID
using StringBuilder = Java.Lang.StringBuilder;
using Android.App;
using Android.Content;
using Android.OS;
using Java.IO;
using Java.Lang;
using Java.Net;
#endif

static class Soundloader
{
    private static readonly string Tag = nameof(Soundloader);

    public static bool MIsShared = false;
    public static bool MFailedShowInter = false;

    public static readonly string[] CharsToRemove = new string[] { "\\", ":", "*", "?", "<", ">", "|", ".", "#" };
    private static readonly string TWITTER_PLAYER = "twitter:player",
        BASE_THUMBNAIL_URL = "i1.sndcdn.com/art",
        BASE_THUMBNAIL_URL_ALT = "i1.sndcdn.com/a",
        FLAG_BEGIN_STREAM_ID = "media/soundcloud:tracks:",
        FLAG_END_STREAM_ID = "/stream",
        STREAM_URL_BASE = "https://api-v2.soundcloud.com/media/soundcloud:tracks:",
        STREAM_URL_END = "/stream/hls",
        FLAG_CLIENT_ID = "client_id:u?";
    private static readonly int OFFSET_CLIENT_ID = 13,
        OFFSET_HTTPS = 8,
        RETRY_DELAY = 2000;
    public static List<List<string>> MTags = new List<List<string>>();
    public static List<string> MM3uUrls = new List<string>();
    public static List<string> MFullstreamUrls = new List<string>();
    public static string MM3uUrl = "";

    public static List<string> mMp3Urls = new List<string>();
    public static List<string> MMp3Urls
    {
        get { return mMp3Urls; }
        set
        {
            if (value == mMp3Urls)
            {
                return;
            }
            mMp3Urls = value;
            Preferences.Default.Set("MP3_URLS", string.Join("|", mMp3Urls));
        }
    }

    public static int MCountChunks = 0;
    public static int MCountChunksFinal = 0;
    public static bool MIsPlaylist = false;
    public static int MCountTracks = 0;
    public static int MCountTracksFinal = 0;
    public static string AbsPathDocs = "";
    public static string AbsPathDocsTemp = "";
    public static string MFilePath = "";
    public static string MThumbnailFilename = "";

    public static string MClientId = "";
    public static string MStreamUrl = "";
    public static string MFullStreamUrl = "";
    public static string MPlayerUrl = "";
    public static string mM3uFileName = "track_playlist";
    

    // RESET
    public static void PrepareFileDirs()
    {
        Console.WriteLine($"{Tag}: PrepareFileDirs");

        Soundloader.AbsPathDocs =
            Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDocuments);
        AbsPathDocsTemp = Soundloader.AbsPathDocs + "/temp";

        // try create docs directory
        Java.IO.File files = new Java.IO.File(AbsPathDocs);
        files.SetWritable(true);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AbsPathDocs));
        }
        catch (SystemException e) 
        {
            Console.WriteLine($"{Tag}: CreateDirectory failed! e={e}");
        }
        
        // try create temp directory
        Java.IO.File temp_files = new Java.IO.File(AbsPathDocsTemp);
        temp_files.SetWritable(true);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AbsPathDocsTemp));
        }
        catch (SystemException e)
        {
            Console.WriteLine($"{Tag}: CreateDirectory failed! e={e}");
        }

        // append / to paths for future usage
        AbsPathDocs += "/";
        AbsPathDocsTemp += "/";

        Console.WriteLine($"{Tag}: AbsPathDocs={AbsPathDocs}");
        Console.WriteLine($"{Tag}: AbsPathDocsTemp={AbsPathDocsTemp}");
    }

    // TODO something, anything about this mess
    public static void ResetVars()
    {
        Console.WriteLine($"{Tag} ResetVars");
        MainPage mp = (MainPage)Shell.Current.CurrentPage;
        MCountChunksFinal = 0;
        MCountChunks = 0;
        MCountTracks = 0;
        MCountTracksFinal = 0;
        MM3uUrl = "";
        MM3uUrls = [];
        MMp3Urls = [];
        MFullstreamUrls = [];
        MTags = new List<List<string>>();
        MStreamUrl = "";
        MPlayerUrl = "";
        MFullStreamUrl = "";
        MFilePath = "";
        mp.MThumbnailUrl = "";
        mp.MTitle = "";
        mp.MArtist = "";
        MThumbnailFilename = "";
        mp.MMessageProgress = "";
        mp.MMessageToast = "";
        MFailedShowInter = false;
    }

    public static void ResetVarsForNext()
    {
        // reset vars, just not everything
        MCountChunksFinal = 0;
        MCountChunks = 0;
        MMp3Urls = [];
        MFilePath = "";
    }

#if ANDROID
    // recursively delete everything in given directory
    public static bool DeleteTempFiles(Java.IO.File temp)
    {
        Console.WriteLine($"{Tag} DeleteTempFiles temp.AbsolutePath={temp.AbsolutePath}");
        if (temp.IsDirectory)
        {
            Java.IO.File[] allContents = temp.ListFiles();
            foreach (Java.IO.File file in allContents)
            {
                DeleteTempFiles(file);
            }
        }
        return temp.Delete();
    }
#endif

    // LOAD
#if ANDROID
    public static async Task  LoadJson(string u)
    {
        Console.WriteLine($"{Tag} LoadJson url={u}");
        HttpURLConnection connection;
        BufferedReader reader;
        string m3uUrl;
        try
        {
            // establish connection
            URL url = new URL(u);
            connection = (HttpURLConnection)url.OpenConnection();
            connection.Connect();

            // get json response as string
            Stream stream = connection.InputStream;
            reader = new BufferedReader(new InputStreamReader(stream));
            StringBuilder builder = new StringBuilder();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                builder.Append(line);
                builder.Append("\n");
            }
            string json = builder.ToString();

            // clean up behind you
            stream.Close();
            reader.Close();
            connection.Disconnect();

            if (u.Contains("com/playlists"))
            {
                // get playlist id
                string playlistId = u.Substring(u.IndexOf("/playlists/") + 11, 7);
                Console.WriteLine($"{Tag} playlistId={playlistId}");

                // get track id(s)
                string ids = "";
                
                while (json.Contains("\"kind\":\"track"))
                {
                    Console.WriteLine($"{Tag} found track");
                    if (ids != "")
                        ids += "%2C";

                    string id = json.Substring(json.IndexOf("\"kind\":\"track") - 11);
                    if (id.StartsWith(":"))
                    {
                        id = id.Substring(1);
                    }
                    id = id.Substring(0, id.IndexOf(","));

                    ids += id;

                    json = json.Substring(json.IndexOf("\"kind\":\"track") + 1);
                }
                Console.WriteLine($"{Tag} ids={ids}");

                // extract appVersion
                string appVersion = "";
                if (u.Contains("app_version="))
                {
                    appVersion = u.Substring(u.IndexOf("app_version=") + 12);
                    if (appVersion.Contains("&"))
                    {
                        appVersion = appVersion.Substring(0, appVersion.IndexOf("&"));
                    }
                }
                Console.WriteLine($"{Tag} appVersion={appVersion}");

                // build full tracks url
                string param = $"ids={ids}&client_id={MClientId}&%5Bobject%20Object%5D=&app_version={appVersion}";
                string fullTracksUrl = $"https://api-v2.soundcloud.com/tracks?{param}";
                Console.WriteLine($"{Tag} fullTracksUrl={fullTracksUrl}");

                LoadJson(fullTracksUrl);
            }
            else if (u.Contains("com/tracks?ids"))
            {
                // trim to tracks
                if (json.Contains("\"tracks\":"))
                {
                    json = json.Substring(json.IndexOf("\"tracks\":"));
                }
                // extract each track
                while (json.Contains("https://api-v2.soundcloud.com"))
                {
                    List<string> tags = new List<string>();
                    // extract thumbnail url
                    string au = "";
                    if (json.Contains("\"artwork_url"))
                    {
                        int start = json.IndexOf("\"artwork_url") + 15;
                        int length = json.IndexOf('"', start) - start;
                        au = json.Substring(start, length);
                        // set to 500x500 quality
                        if (au.Contains("-large."))
                        {
                            au = au.Replace("-large.", "-t500x500.");
                        }
                    }
                    if (au == "")
                    {
                        // default artwork url
                        au = ((MainPage)Shell.Current.CurrentPage).MThumbnailUrl;
                    }
                    Console.WriteLine($"{Tag} artwork_url={au}");
                    tags.Add(au);

                    // extract title (title)
                    string t = "";
                    if (json.Contains("\"title\""))
                    {
                        int start = json.IndexOf("\"title\"") + 9;
                        int length = json.IndexOf('"', start) - start;
                        t = json.Substring(start, length);
                        // TODO? rm sensitive chars
                    }
                    Console.WriteLine($"{Tag} title={t}");
                    tags.Add(t);

                    // extract artist
                    string a = "";
                    if (json.Contains("\"artist\""))
                    {
                        int start = json.IndexOf("\"artist\"") + 10;
                        int length = json.IndexOf('"', start) - start;
                        a = json.Substring(start, length);
                    }
                    if (a == "")
                    {
                        // default artist
                        a = ((MainPage)Shell.Current.CurrentPage).MArtist;
                    }
                    Console.WriteLine($"{Tag} artist={a}");
                    tags.Add(a);

                    // extract album
                    string at = "";
                    if (json.Contains("\"album_title\""))
                    {
                        int start = json.IndexOf("\"album_title\"") + 15;
                        int length = json.IndexOf('"', start) - start;
                        at = json.Substring(start, length);
                    }
                    if (at == "")
                    {
                        // default album title
                        at = ((MainPage)Shell.Current.CurrentPage).MTitle;
                    }
                    Console.WriteLine($"{Tag} album_title={at}");
                    tags.Add(at);

                    // extract genre
                    string g = "";
                    if (json.Contains("\"genre\""))
                    {
                        int start = json.IndexOf("\"genre\"") + 9;
                        int length = json.IndexOf('"', start) - start;
                        g = json.Substring(start, length);
                    }
                    Console.WriteLine($"{Tag} genre={g}");
                    tags.Add(g);

                    // extract release date
                    string ca = "";
                    if (json.Contains("\"created_at\""))
                    {
                        int start = json.IndexOf("\"created_at\"") + 14;
                        int length = 4;
                        ca = json.Substring(start, length);
                    }
                    Console.WriteLine($"{Tag} created_at={ca}");
                    tags.Add(ca);

                    // add track tags
                    MTags.Add(tags);

                    // extract stream url
                    int s = json.IndexOf("https://api-v2.soundcloud.com");
                    int l = json.IndexOf('"', s) - s;
                    string fullstreamUrl = json.Substring(s, l) + $"?client_id={MClientId}";
                    MFullstreamUrls.Add(fullstreamUrl);
                    Console.WriteLine($"{Tag} adding fullstreamUrl={fullstreamUrl}");

                    // trim to first stream url (only matters first run...)
                    json = json.Substring(json.IndexOf("https://api-v2.soundcloud.com") + 1);
                    // trim to next transcodings
                    if (json.Contains("transcodings"))
                    {
                        json = json.Substring(json.IndexOf("transcodings") + 1);
                    }
                    else
                    {
                        if (json.Contains("https://api-v2.soundcloud.com"))
                        {
                            // trim remaining stream urls (to end loop)
                            json = json.Substring(json.LastIndexOf("https://api-v2.soundcloud.com") + 1);
                        }
                    }
                }
                Console.WriteLine($"{Tag} MFullstreamUrls.Count={MFullstreamUrls.Count}");
                // load fullstreamUrls
                foreach (string streamUrl in MFullstreamUrls)
                {
                    await LoadJson(streamUrl);
                }
            }
            else
            {
                // extract m3u url
                if (json.Contains("https://"))
                {
                    int s = json.IndexOf("https://");
                    int l = json.IndexOf('"', s) - s;
                    m3uUrl = json.Substring(s, l);
                    if (m3uUrl.EndsWith("/") || m3uUrl.EndsWith("\\"))
                    {
                        m3uUrl = m3uUrl.Substring(0, m3uUrl.Length - 1);
                    }
                    

                    if (MFullstreamUrls.Count == 0)
                    {
                        MM3uUrl = m3uUrl;
                        Console.WriteLine($"{Tag} MM3uUrl={MM3uUrl}");

                        // loaded track
                        MCountTracksFinal = 1;
                        MM3uUrl = m3uUrl;

                        // run on ui thread
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ((MainPage)Shell.Current.CurrentPage).ShowPreviewUI();
                        });
                    }
                    else
                    {
                        MM3uUrl = m3uUrl;
                        Console.WriteLine($"{Tag} MM3uUrls.Add({MM3uUrl})");
                        MM3uUrls.Add(m3uUrl);
                        MCountTracksFinal = MM3uUrls.Count;
                        Console.WriteLine($"{Tag} MCountTracksFinal={MCountTracksFinal} MM3uUrls.Count={MM3uUrls.Count} MFullstreamUrls.Count={MFullstreamUrls.Count}");
                        if (MFullstreamUrls.Count == MM3uUrls.Count)
                        {
                            // finished loading playlist
                            MM3uUrl = MM3uUrls[0];
                            MM3uUrls.Remove(MM3uUrls[0]);
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                ((MainPage)Shell.Current.CurrentPage).ShowPreviewUI();
                            });
                        }
                        
                    }
                }
                else
                {
                    Console.WriteLine($"{Tag} missing m3u url");
                }
            }

        }
        catch (Java.IO.IOException e)
        {
            Console.WriteLine($"{Tag} connection failed (full stream url)");
        }
        catch (NullPointerException e)
        {
            Console.WriteLine($"{Tag} {"null pointer"}");
        }
    }
#endif

    public static IEnumerable<string> Split(string str, int chunkSize)
    {
        return Enumerable.Range(0, str.Length / chunkSize)
            .Select(i => str.Substring(i * chunkSize, chunkSize));
    }

    // EXTRACT
    public static void ExtractPlayerUrl(string head)
    {
        Console.WriteLine($"ExtractPlayerUrl");
        int s, l;
        if (head.Contains(TWITTER_PLAYER))
        {
            s = head.IndexOf(TWITTER_PLAYER) + TWITTER_PLAYER.Length;
            s = head.IndexOf("content", s) + 9;
            l = head.IndexOf('"', s) - s;
            string playerUrl = head.Substring(s, l);
            if (playerUrl.Contains("\\"))
                playerUrl = playerUrl.Replace("\\", "");

            MPlayerUrl = playerUrl;
            Console.WriteLine($"{Tag} MPlayerUrl={MPlayerUrl}");
        }
        else
        {
            Console.WriteLine($"{Tag} Missing player url!");
        }
    }
    public static string ExtractThumbnailUrl(string html)
    {
        int s, l;
        if (html.Contains(BASE_THUMBNAIL_URL) || html.Contains(BASE_THUMBNAIL_URL_ALT))
        {
            if (html.Contains(BASE_THUMBNAIL_URL))
            {
                s = html.LastIndexOf(BASE_THUMBNAIL_URL) - OFFSET_HTTPS;
            }
            else
            {
                s = html.LastIndexOf(BASE_THUMBNAIL_URL_ALT) - OFFSET_HTTPS;
            }
            l = html.IndexOf('"', s) - s;
            string ut = html.Substring(s, l);
            if (ut.EndsWith("/") || ut.EndsWith("\\"))
            {
                ut = ut.Substring(0, ut.Length - 1);
            }

            // set to 500x500 quality
            if (ut.Contains("-large."))
            {
                ut = ut.Replace("-large.", "-t500x500.");
            }

            // set thumbnail filename
            if (ut.Contains(".jpg"))
            {
                MThumbnailFilename = "thumbnail.jpg";
            }
            else
            {
                MThumbnailFilename = "thumbnail.png";
            }
            Console.WriteLine($"{Tag} MThumbnailFilename={MThumbnailFilename}");

            return ut;
        }
        else
        {
            Console.WriteLine($"{Tag} Missing thumbnail url!");
            return "";
        }

    }
    public static void ExtractStreamUrl(string html)
    {
        int s, l;
        if (html.Contains(FLAG_BEGIN_STREAM_ID))
        {

            s = html.IndexOf(FLAG_BEGIN_STREAM_ID);
            l = html.LastIndexOf(FLAG_BEGIN_STREAM_ID) - s;
            html = html.Substring(s, l);
        }
        if (html.Contains(FLAG_BEGIN_STREAM_ID) && html.Contains(FLAG_END_STREAM_ID))
        {
            // extract id
            s = html.LastIndexOf(FLAG_BEGIN_STREAM_ID) + FLAG_BEGIN_STREAM_ID.Length;
            l = html.IndexOf(FLAG_END_STREAM_ID, s) - s;
            string id = html.Substring(s, l);

            // insert id into stream url
            string us = STREAM_URL_BASE + id + STREAM_URL_END;
            MStreamUrl = us;
        }
        Console.WriteLine($"{Tag} MStreamUrl={MStreamUrl}");
    }

    public static List<string> ExtractMp3Urls(string abs_path_m3u)
    {
        List<string> urls = new List<string>();
        try
        {
            Java.IO.File m3u = new Java.IO.File(abs_path_m3u);
            BufferedReader br = new BufferedReader(new FileReader(m3u));
            string line;

            // Parse file line-by-line, extracting chunk urls
            while ((line = br.ReadLine()) != null)
            {
                // every other non-comment line is the start of a new url
                if (!line.StartsWith("#"))
                {
                    urls.Add(line);
                }
            }
            br.Close();
            if (!m3u.Delete())
            {
                Console.WriteLine($"{Tag} delete playlist failed!"); ;
            }
        }
        catch (Java.IO.FileNotFoundException e)
        {
            Console.WriteLine($"{Tag} {e}");
        }
        catch (Java.IO.IOException e)
        {
            Console.WriteLine($"{Tag} {e}");
        }

        return urls;
    }

    // TODO download in IOS


    // DOWNLOAD
    public static async Task DownloadM3u()
    {
        Console.WriteLine($"{Tag} DownloadM3u");

        // TODO start download on IOS


        if (MM3uUrl != "")
        {
            MainPage mp = (MainPage)Shell.Current.CurrentPage;
            DownloadManager downloadManager = (DownloadManager)
                    MainActivity.ActivityCurrent.GetSystemService(Context.DownloadService);

            Android.Net.Uri fileUri = Android.Net.Uri.Parse(MM3uUrl);
            string fileDir = Android.OS.Environment.DirectoryDocuments;

            Console.WriteLine($"{Tag} downloading url={MM3uUrl} mM3uFileName={mM3uFileName}");

            DownloadManager.Request request = new DownloadManager.Request(fileUri);
            request.SetTitle("track chunklist");
            request.SetDescription("");
            request.SetDestinationInExternalPublicDir(fileDir, "/temp/" + mM3uFileName + ".m3u");
            downloadManager.Enqueue(request);
        }
        else
        {
            Console.WriteLine($"{Tag} M3uUrls is empty!");
        }
    }

    public static async Task DownloadMp3(int num)
    {
        Console.WriteLine($"{Tag} DownloadMp3");

        if (MMp3Urls.Count > 0)
        {
            DownloadManager downloadManager = (DownloadManager)
                    MainActivity.ActivityCurrent.GetSystemService(Context.DownloadService);
            string targetUrl = MMp3Urls[num];
            Android.Net.Uri fileUri = Android.Net.Uri.Parse(targetUrl);
            string fileDir = Android.OS.Environment.DirectoryDocuments;
            string fileName = "s" + num;

            Console.WriteLine($"{Tag} targetUrl={targetUrl} fileName={fileName}");

            DownloadManager.Request request = new DownloadManager.Request(fileUri);
            request.SetTitle("track chunk");
            request.SetDescription("");
            request.SetDestinationInExternalPublicDir(
                fileDir, "/temp/" + fileName + ".mp3");
            downloadManager.Enqueue(request);
        }
        else
        {
            Console.WriteLine($"{Tag} Mp3Urls is empty!");
        }
    }

    public static async Task DownloadImage()
    {
        Console.WriteLine($"{Tag} DownloadImage");
        MainPage mp = (MainPage)Shell.Current.CurrentPage;
        if (mp.MThumbnailUrl != "")
        {
            DownloadManager downloadManager = (DownloadManager)
                    MainActivity.ActivityCurrent.GetSystemService(Context.DownloadService);
            string targetUrl = mp.MThumbnailUrl;
            Android.Net.Uri fileUri = Android.Net.Uri.Parse(targetUrl);
            string fileName = MThumbnailFilename;
            string fileDir = Android.OS.Environment.DirectoryDocuments;

            /*// TODO set artwork from tags
            if (MTags[0] != null && MTags[0][0] != null && MTags[0][0] != "")
            {
                Console.WriteLine($"{Tag} setting targetUrl to MTags[0][0]={MTags[0][0]}");
                targetUrl = MTags[0][0];
                if (targetUrl.Contains(".png"))
                {
                    fileName = "thumbnail.png";
                } else
                {
                    fileName = "thumbnail.jpg";
                }
            }
            */

            Console.WriteLine($"{Tag} targetUrl={targetUrl} fileName={fileName}");

            DownloadManager.Request request = new DownloadManager.Request(fileUri);
            request.SetTitle("album artwork");
            request.SetDescription("");
            request.SetDestinationInExternalPublicDir(
                fileDir, "/temp/" + fileName);
            downloadManager.Enqueue(request);
        }
        else
        {
            Console.WriteLine($"{Tag} MThumbnailUrl is empty!");
        }
    }

    public static async Task<string> ConcatMp3()
    {
        Console.WriteLine($"{Tag} ConcatMp3");

        // build destination file path
        int nameOffset = 0;
        string filename = ((MainPage)Shell.Current.CurrentPage).MTitle;
        string fileExt = ".mp3";
        string destPath = AbsPathDocs + filename + fileExt;

#if ANDROID
        // prevent overwrite
        while (new Java.IO.File(destPath).Exists())
        {
            ++nameOffset;
            destPath = AbsPathDocs + filename + nameOffset + fileExt;
        }
        MFilePath = destPath;
#endif

        // build chunk paths
        string chunkPathBase = AbsPathDocsTemp + "s";
        List<string> chunkPaths = [];
        string chunkPathZero = chunkPathBase + "0" + fileExt;
        for (int c = 1; c < MCountChunksFinal; ++c)
        {
            string path = chunkPathBase + c + fileExt;
            chunkPaths.Add(path);
        }
        Console.WriteLine($"{Tag} absFilePath={destPath} chunkPathZero={chunkPathZero} " +
            $"chunkPaths[0]={chunkPaths[0]} chunkPaths.Count={chunkPaths.Count}");

        // concat files
        try
        {
            using var fs = File.OpenWrite(destPath);
            var buffer = File.ReadAllBytes(chunkPathZero);
            fs.Write(buffer, 0, buffer.Length);
            foreach (var path in chunkPaths)
            {
                buffer = File.ReadAllBytes(path);
                fs.Write(buffer, 0, buffer.Length);
            }
            fs.Flush();
            Console.WriteLine($"{Tag} concat finished");
            //fs.Close();
        } catch (System.Exception e)
        {
            Console.WriteLine($"{Tag} concat finished with exception! e={e}");
        }
        return destPath;
    }

    public static async Task SetTags(string absFilePath)
    {
        Console.WriteLine($"{Tag} SetTags absFilePath={absFilePath}");
        try
        {
            TagLib.File f = TagLib.File.Create(absFilePath);
            string thumbPath = AbsPathDocsTemp + MThumbnailFilename;
            try
            {
                Console.WriteLine($"{Tag} trying setting MTags tags");
                /*
                if (MTags[0][0].Contains(".jpg"))
                {
                    thumbPath = AbsPathDocsTemp + "thumbnail.jpg";
                }
                else
                {
                    thumbPath = AbsPathDocsTemp + "thumbnail.png";
                }
                */
                f.Tag.Title = MTags[0][1];
                f.Tag.Performers = [MTags[0][2]];
                f.Tag.Album = MTags[0][3];
                f.Tag.Genres = [MTags[0][4]];
                f.Tag.Year = uint.Parse(MTags[0][5]);
                MTags.Remove(MTags[0]);
            } catch (System.Exception)
            {
                Console.WriteLine($"{Tag} setting basic tags");
                f.Tag.Title = ((MainPage)Shell.Current.CurrentPage).MTitle;
                f.Tag.Performers = [((MainPage)Shell.Current.CurrentPage).MArtist];
            }
            Console.WriteLine($"{Tag} finished setting tags");
            IPicture newArt = new Picture(thumbPath);
            f.Tag.Pictures = [newArt];
            Console.WriteLine($"{Tag} finished setting album artwork");

            // save file
            f.Save();
            Console.WriteLine($"{Tag} saved file");
        } catch (System.Exception e)
        {
            Console.WriteLine($"{Tag} failed to write tags & art! e={e}");
        }
    }

    public static async Task ScanMp3()
    {
#if ANDROID
        // scan media file
        Console.WriteLine($"{Tag} scanning new media file at: MFilePath={MFilePath}");
        Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + MFilePath);
        Intent scanFileIntent = new Intent(Intent.ActionMediaScannerScanFile, uri);
        MainActivity.ActivityCurrent.SendBroadcast(scanFileIntent);
#endif
    }

}
