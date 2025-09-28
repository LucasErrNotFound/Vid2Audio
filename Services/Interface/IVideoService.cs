using System.Collections.ObjectModel;
using Vid2Audio.ViewModels;

namespace Vid2Audio.Services.Interface;

public interface IVideoService
{
    ObservableCollection<VideoItem> VideoList { get; set; }
    void AddVideo(VideoItem videoItem);
    void RemoveVideo(VideoItem videoItem);
    void ClearVideos();
}