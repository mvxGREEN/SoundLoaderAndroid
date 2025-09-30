# SoundLoader: Soundcloud Downloader

<img width="72" height="72" alt="ic_launcher_soundloader_round" src="https://github.com/user-attachments/assets/1cd86fc9-3afa-415c-8bcf-6d0cd810ab82" />  

## About

SoundLoader is a simple music downloader app, built for Android with the .NET MAUI framework.  It accepts a Soundcloud URL (track, album or playlist) and downloads the corresponding track(s) to your device's local storage.


## Features

*  URL-to-MP3 Downloader
*  Batch Downloader (Playlists & Albums)
*  Share-to-Download
*  Run in Background
*  Paste Button
*  Thumbnail
*  Metadata
*  Original Quality

## Screenshots

<img width="405" height="720" alt="Samsung Galaxy S21 Ultra Screenshot 1" src="https://github.com/user-attachments/assets/ae7b687a-7609-4f2a-b28a-54cd3d7710dd" />
<img width="405" height="720" alt="Samsung Galaxy S21 Ultra Screenshot 2" src="https://github.com/user-attachments/assets/cc057454-9e08-4ad9-b9d1-49dbfd81e113" />


## How To Install

1.  Clone [SoundLoaderAndroid](https://github.com/mvxGREEN/SoundLoaderAndroid) repo from Github.
2.  Open solution file in Visual Studio.
3.  Build app
4.  Run on your physical or emulated Android device.


## How To Use

1.  Copy desired URL
2.  Paste into SoundLoader
3.  Tap download button

OR,

1.  Open desired track
2.  Tap "Share"
3.  Tap "SoundLoader"

Done!  File will be saved to your device's Documents directory.


## [Demo](https://youtu.be/Evi0wVs-WLI?si=z8fdNlIfUhn9m3Xa)


## How It Works

*\*User inputs URL\**
1.  Load page HTML at given URL
2.  Extract player URL from HTML
3.  Load player URL in WebView
4.  Extract HTML from loaded page in webview
5.  Parse HTML for stream URL and track metadata (title, artist, thumbnail URL)
6.  Intercept and parse request URLs for 'client_id' parameter
7.  Append 'client_id' parameter to stream URL
8.  Download .M3U8 file from new stream URL 
9.  Parse .M3U8 file for .TS URLs
10.  Download .TS files
11.  Concatenate .TS files into a single MP3 file
12.  Delete temporary files (M3U8 and TS files)


## Contributing

We love contributions <3
