using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Vid2Audio.Services.Interface;
using Vid2Audio.ViewModels;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace Vid2Audio.Services;

public class VideoService : IVideoService
{
    public required ObservableCollection<VideoItem> VideoList { get; set; }

    public void AddVideo(VideoItem videoItem)
    {
        VideoList.Add(videoItem);
    }
    
    public async Task DownloadVideo(VideoItem videoItem)
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
            "M4A" => AudioConversionFormat.Aac,
            //"OGG" => AudioConversionFormat.Vorbis,
            _ => AudioConversionFormat.Mp3
        };
        
        var res = await ytdl.RunAudioDownload(videoItem.VideoUrl, audioFormat);
        Debug.WriteLine(res.Success ? "Success" : "Error");
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