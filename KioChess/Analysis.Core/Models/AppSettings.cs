namespace Analysis.Core.Models;

public class AppSettings
{
    public string StockfishPath { get; set; } = @"C:\ChessEngines\stockfish\stockfish.exe";
    public string Theme         { get; set; } = "ClassicWood";
    public string PieceSet      { get; set; } = "Classic";
    public bool   SoundEnabled  { get; set; } = true;
    public double SoundVolume   { get; set; } = 0.8;
    public string AnimationSpeed { get; set; } = "Normal";
}
