<img width="72" height="72" alt="ic_launcher_soundloader_round" src="https://github.com/user-attachments/assets/1cd86fc9-3afa-415c-8bcf-6d0cd810ab82" />  
# SoundLoader: Soundcloud Downloader

<img width="1024" height="500" alt="play_store_feature_graphic" src="https://github.com/user-attachments/assets/6aa0246a-9953-493b-88e6-25e871157643" />

## About

SoundLoader is a simple music downloader app, built for Android with the .NET MAUI framework.  It accepts a Soundcloud URL (track, album or playlist) and downloads the corresponding track(s) to your device's local storage.


## Features

*  URL-to-MP3 Downloader
*  Batch Downloader (Playlists & Albums)
*  Share-to-Download
*  Run in Background
*  Thumbnail
*  Metadata
*  Original Quality


## Screenshots

<img width="405" height="720" alt="Samsung Galaxy S21 Ultra Screenshot 1" src="https://github.com/user-attachments/assets/ae7b687a-7609-4f2a-b28a-54cd3d7710dd" />
<img width="405" height="720" alt="Samsung Galaxy S21 Ultra Screenshot 2" src="https://github.com/user-attachments/assets/cc057454-9e08-4ad9-b9d1-49dbfd81e113" />


## How To Install

1.  Clone SoundLoaderAndroid from Github.
2.  Open solution in Visual Studio.
3.  Build and run on your Android device.


## How To Use

1.  Copy desired URL
2.  Paste into SoundLoader
OR,
1.  Open desired track and tap "Share"
2.  Tap "SoundLoader"

Done!  File will be saved to your device's Documents directory.


## [Demo](https://youtu.be/Evi0wVs-WLI?si=z8fdNlIfUhn9m3Xa)


## How It Works

*  User inputs URL...
*  Load page HTML at given URL
*  Extract player URL from HTML (https://w.soundcloud.com...)
*  Load player URL in WebView
*  Load and parses HTML from webview
*  Extract track metadata (title, artist, thumbnail URL, etc.)
*  Extract stream URL
*  Parse request URL's for client_id
*  Append client_id parameter to stream URL
*  Download .M3U8 file from stream URL
*  Parse .M3U8 file for .TS file URL's
*  Download .TS files from each URL
*  Concatenate .TS files into a complete MP3 file


## Contributing

We love contributions <3
