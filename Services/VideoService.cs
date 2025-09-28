using System.Collections.ObjectModel;
using Vid2Audio.Services.Interface;
using Vid2Audio.ViewModels;

namespace Vid2Audio.Services;

public class VideoService : IVideoService
{
    public required ObservableCollection<VideoItem> VideoList { get; set; }

    public void AddVideo(VideoItem videoItem)
    {
        VideoList.Add(videoItem);
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