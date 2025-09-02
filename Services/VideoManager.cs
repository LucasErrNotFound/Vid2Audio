using System.Collections.ObjectModel;
using Vid2Audio.ViewModels;

namespace Vid2Audio.Services;

public class VideoManager
{
    public static ObservableCollection<VideoItem> VideoList { get; } = [];
    
    public static void AddVideo(VideoItem videoItem)
    {
        VideoList.Add(videoItem);
    }
    
    public static void RemoveVideo(VideoItem videoItem)
    {
        VideoList.Remove(videoItem);
    }
    
    public static void ClearVideos()
    {
        VideoList.Clear();
    }
}