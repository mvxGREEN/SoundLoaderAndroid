# SoundLoader: Soundcloud Downloader

<img width="72" height="72" alt="ic_launcher_soundloader_round" src="https://github.com/user-attachments/assets/1cd86fc9-3afa-415c-8bcf-6d0cd810ab82" />  


## About

SoundLoader is a simple music downloader app, built for Android with the .NET MAUI framework.  It accepts a Soundcloud URL (track, album or playlist) and downloads the corresponding track(s) to your device's local storage.


## Screenshots

<img width="405" height="720" alt="Samsung Galaxy S21 Ultra Screenshot 1" src="https://github.com/user-attachments/assets/ae7b687a-7609-4f2a-b28a-54cd3d7710dd" />
<img width="405" height="720" alt="Samsung Galaxy S21 Ultra Screenshot 2" src="https://github.com/user-attachments/assets/cc057454-9e08-4ad9-b9d1-49dbfd81e113" />


## Features

*  URL-to-MP3 Downloader
*  Batch Downloader (Playlists & Albums)
*  Share-to-Download
*  Run in Background
*  Thumbnail
*  Metadata
*  Original quality sound


## How To Install

1.  Clone SoundLoaderAndroid from Github.
2.  Open solution in Visual Studio.
3.  Build and run it on your Android device.


## How To Use

1.  Copy URL of a track and paste in SoundLoader.
2.  OR tap "share" then "SoundLoader" in Soundcloud
3.  Tap download button

Done!  File will be downloaded to internal storage, in the Documents directory.


## [Demo](https://youtu.be/Evi0wVs-WLI?si=z8fdNlIfUhn9m3Xa)


## How It Works

*  User inputs URL
*  App loads URL HTML
*  Extracts 'player' URL from HTML (https://w.soundcloud.com...)
*  Loads player url in WebView
*  Intercepts "client_id" value from request URL's, OR from 'widget-9' request
*  Parses HTML from webview
*  Extracts metadata (title, artist, thumbnail URL)
*  Extracts 'stream' URL
*  Appends client_id key and value to stream URL
*  Downloads m3u8 file from stream URL
*  Parses M3U8 file for MP3 chunk files' URL's (TS files)
*  Downloads chunk files from their URL's
*  Concatenates chunk files into complete track MP3 file


## Contributing

We love contributions <3
