using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vid2Audio.ViewModels;
using YoutubeDLSharp.Metadata;

namespace Vid2Audio.Services.Interface;

public interface IVideoService
{
    ObservableCollection<VideoItem> VideoList { get; set; }
    void AddVideo(VideoItem videoItem);
    bool ValidateVideoUrl(string videoUrl);
    Task<VideoData?> FetchVideoDataAsync(string videoUrl);
    Task<bool> DownloadVideoAsync(VideoItem videoItem);
    Task<bool> DownloadMultipleVideosAsync(IEnumerable<VideoItem> videoItems);

    void RemoveVideo(VideoItem videoItem);
    void ClearVideos();
}