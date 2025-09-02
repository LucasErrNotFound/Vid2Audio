using System.Diagnostics;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace Vid2Audio.VideoConverter.Youtube;

public class YoutubeConverter
{
    public static async Task DownloadYoutube(string youtubeUrl)
    {
        Debug.WriteLine(youtubeUrl);
        var ytdl = new YoutubeDL
        {
            YoutubeDLPath = @"Binaries\yt-dlp.exe",
            FFmpegPath = "ffmpeg.exe",
            OutputFolder = "Downloads"
        };
        var res = await ytdl.RunVideoDataFetch(youtubeUrl);
        VideoData video = res.Data;
        
        string title = video.Title;
        string uploader = video.Uploader;
        long? views = video.ViewCount;
        
        Debug.WriteLine(title);
        Debug.WriteLine(uploader);
        Debug.WriteLine(views);
        Debug.WriteLine(video.Thumbnail);
    }
}