using System;
using System.Diagnostics;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace Vid2Audio.VideoConverter.Youtube;

public class YoutubeConverter
{
    public static async Task<VideoData?> GetVideoData(string youtubeUrl)
    {
        try
        {
            Debug.WriteLine($"Getting video data for {youtubeUrl}");
            
            var ytdl = new YoutubeDL
            {
                YoutubeDLPath = @"Binaries\yt-dlp.exe",
                FFmpegPath = "ffmpeg.exe",
                OutputFolder = "Downloads"
            };
            var res = await ytdl.RunVideoDataFetch(youtubeUrl);

            if (!res.Success || res.Data is null) return null;
            
            VideoData video = res.Data;
            return video;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to fetch video data: {ex.Message}");
            return null;
        }
    }
}