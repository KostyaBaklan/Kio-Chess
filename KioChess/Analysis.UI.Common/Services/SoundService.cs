using Analysis.Core.Interfaces;
using System.IO;
using System.Media;
using System.Windows;

namespace Analysis.UI.Common.Services;

/// <summary>
/// WPF implementation of sound service that plays WAV files from embedded resources.
/// Sounds are played asynchronously and volume is controlled via system mixer.
/// </summary>
public class SoundService : ISoundService
{
    private bool _enabled = true;
    private double _volume = 0.8;
    private readonly Dictionary<string, Stream> _soundStreams = new();
    private readonly object _lock = new();

    public SoundService()
    {
        LoadSoundResources();
    }

    public void PlayMove() => PlayAsync("move");
    public void PlayCapture() => PlayAsync("capture");
    public void PlayCastle() => PlayAsync("castle");
    public void PlayCheck() => PlayAsync("check");
    public void PlayPromotion() => PlayAsync("promote");
    public void PlayWin() => PlayAsync("win");
    public void PlayLose() => PlayAsync("lose");
    public void PlayDraw() => PlayAsync("draw");
    public void PlayClick() => PlayAsync("click");

    public void SetEnabled(bool enabled) => _enabled = enabled;
    public void SetVolume(double volume) => _volume = Math.Clamp(volume, 0.0, 1.0);

    private void LoadSoundResources()
    {
        var soundNames = new[]
        {
            "move", "capture", "castle", "check", 
            "win", "lose", "draw", "click",
            "error", "notify", "promote"
        };

        foreach (var name in soundNames)
        {
            try
            {
                var resourcePath = $"Sounds/{name}.wav";
                var uri = new Uri($"pack://application:,,,/Analysis.UI.Common;component/{resourcePath}");
                var resourceInfo = Application.GetResourceStream(uri);
                
                if (resourceInfo != null)
                {
                    // Create a MemoryStream copy so we can replay the sound multiple times
                    var memoryStream = new MemoryStream();
                    resourceInfo.Stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    _soundStreams[name] = memoryStream;
                }
                else
                {
                    _soundStreams[name] = null;
                }
            }
            catch
            {
                _soundStreams[name] = null;
            }
        }
    }

    private void PlayAsync(string name)
    {
        if (!_enabled) return;

        Task.Run(() =>
        {
            try
            {
                if (_soundStreams.TryGetValue(name, out var stream) && stream != null)
                {
                    // Create a new MemoryStream for this playback instance
                    var playbackStream = new MemoryStream();
                    lock (_lock)
                    {
                        stream.Position = 0;
                        stream.CopyTo(playbackStream);
                    }
                    playbackStream.Position = 0;
                    
                    using var player = new SoundPlayer(playbackStream);
                    player.PlaySync();
                }
            }
            catch
            {
                // Ignore sound playback errors
            }
        });
    }
}
