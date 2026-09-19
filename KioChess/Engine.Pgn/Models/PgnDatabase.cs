namespace Engine.Pgn.Models;

public class PgnDatabase
{
    public List<PgnGame> Games { get; set; }

    public PgnDatabase()
    {
        Games = new List<PgnGame>();
    }

    public void AddGame(PgnGame game)
    {
        if (game != null)
        {
            Games.Add(game);
        }
    }
}
