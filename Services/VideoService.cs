using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vid2Audio.Services.Interface;
using Vid2Audio.ViewModels;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace Vid2Audio.Services;

public class VideoService : IVideoService
{
    public required ObservableCollection<VideoItem> VideoList { get; set; }

    public void AddVideo(VideoItem videoItem)
    {
        VideoList.Add(videoItem);
    }

    public bool ValidateVideoUrl(string videoUrl)
    {
        if (string.IsNullOrWhiteSpace(videoUrl))
            return false;

        return Uri.TryCreate(videoUrl, UriKind.Absolute, out var uri) 
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public async Task<VideoData?> FetchVideoDataAsync(string videoUrl)
    {
        var ytdl = new YoutubeDL { YoutubeDLPath = @"Binaries\yt-dlp.exe" };
        var result = await ytdl.RunVideoDataFetch(videoUrl);
        
        if (!result.Success || result.Data is null) return null;
        VideoData videoData = result.Data;
        return videoData;
    }
    
    public async Task<bool> DownloadVideoAsync(VideoItem videoItem)
    {
        var ytdl = new YoutubeDL
        {
            YoutubeDLPath = @"Binaries\yt-dlp.exe",
            FFmpegPath = "ffmpeg.exe",
            OutputFolder = "Downloads"
        };

        var audioFormat = videoItem.SelectedAudioFormat switch
        {
            "MP3" => AudioConversionFormat.Mp3,
            "WAV" => AudioConversionFormat.Wav,
            "FLAC" => AudioConversionFormat.Flac,
            "M4A" => AudioConversionFormat.M4a,
            "AAC" => AudioConversionFormat.Aac,
            _ => AudioConversionFormat.Mp3
        };
        
        var res = await ytdl.RunAudioDownload(videoItem.VideoUrl, audioFormat);
        return res.Success;
    }
    
    public void RemoveVideo(VideoItem videoItem)
    {
        VideoList.Remove(videoItem);
    }
    
    public void ClearVideos()
    {
        VideoList.Clear();
    }
}