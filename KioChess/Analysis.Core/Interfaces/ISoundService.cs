namespace Analysis.Core.Interfaces;

public interface ISoundService
{
    void PlayMove();
    void PlayCapture();
    void PlayCastle();
    void PlayCheck();
    void PlayPromotion();
    void PlayWin();
    void PlayLose();
    void PlayDraw();
    void PlayClick();
    void SetEnabled(bool enabled);
    void SetVolume(double volume);
}
