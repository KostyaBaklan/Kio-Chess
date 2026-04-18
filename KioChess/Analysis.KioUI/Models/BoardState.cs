namespace Analysis.KioUI.Models;

public enum BoardState : byte
{
    /// <summary>The board accepts user clicks and move selection.</summary>
    Interactive,
    /// <summary>Read-only — used in the Analysis tab when stepping through moves.</summary>
    ReadOnly,
    /// <summary>Waiting for the engine to reply; clicks are ignored.</summary>
    EngineThinking
}
