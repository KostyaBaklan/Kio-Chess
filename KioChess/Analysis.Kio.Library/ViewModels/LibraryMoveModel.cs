namespace Analysis.Kio.Library.ViewModels;
#region Models

public class LibraryMoveModel : BindableBase
{
    public int Number { get; set; }
    public string MoveNumber { get; set; } = string.Empty;
    public string Move { get; set; } = string.Empty;
    public bool IsWhite { get; set; }
}

#endregion
