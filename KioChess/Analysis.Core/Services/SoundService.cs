using Analysis.Core.Interfaces;

namespace Analysis.Core.Services;

/// <summary>
/// Stub implementation — sound files are added in Phase 6.
/// Each Play* method is a no-op until WAV assets are embedded.
/// </summary>
public class SoundService : ISoundService
{
    private bool _enabled = true;
    private double _volume = 0.8;

    public void PlayMove() => Play("move");
    public void PlayCapture() => Play("capture");
    public void PlayCastle() => Play("castle");
    public void PlayCheck() => Play("check");
    public void PlayPromotion() => Play("promote");
    public void PlayWin() => Play("win");
    public void PlayLose() => Play("lose");
    public void PlayDraw() => Play("draw");
    public void PlayClick() => Play("click");

    public void SetEnabled(bool enabled) => _enabled = enabled;
    public void SetVolume(double volume) => _volume = Math.Clamp(volume, 0.0, 1.0);

    private void Play(string name)
    {
        if (!_enabled) return;
        // Sound file playback will be wired in Phase 6 when WAV assets are added.
        _ = name;
    }
}
