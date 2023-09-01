using Engine.Book.Interfaces;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using OpeningMentor.Chess.Model;
using OpeningMentor.Chess.Model.MoveText;
using OpeningMentor.Chess.Pgn;
using Tools.Common;

internal class Program
{
    private static int _depth;
    private static Dictionary<string, byte> _squares = new Dictionary<string, byte>();
    private static Dictionary<string, byte> _pieces = new Dictionary<string, byte>();

    private static void Main(string[] args)
    {
        Boot.SetUp();

        _depth = Boot.GetService<IConfigurationProvider>().BookConfiguration.SaveDepth;

        for (byte i = 0; i < 64; i++)
        {
            var k = i.AsString().ToLower();
            _squares[k] = i;
        }

        for (byte i = 0; i < 12; i++)
        {
            var p = i.AsEnumString();
            _pieces[p] = i;
        }

        //ProcessFile(@"PGNs\Failures\PGN_Failures_2023_08_31_08_48_36_3557_6f00600f-dc5a-4688-a983-c915f1572210.pgn");

        ProcessArgument(args);
    }

    private static void ProcessFile(string file)
    {
        try
        {
            PgnReader pgnReader = new PgnReader();
            var database = pgnReader.ReadFromFile(file);

            var game = database.Games.FirstOrDefault();

            ProcessGame(game);

            Console.WriteLine("No failure !!!");
        }
        catch (Exception ex)
        {

        }
    }

    private static void ProcessArgument(string[] args)
    {
        Database database = null;
        try
        {
            var buffer = Convert.FromBase64String(args[0]);

            using (var stream = new MemoryStream(buffer))
            {
                stream.Position = 0;

                PgnReader pgnReader = new PgnReader();
                database = pgnReader.ReadFromStream(stream);
            }

            var game = database.Games.FirstOrDefault();

            ProcessGame(game);
        }
        catch (Exception ex)
        {
            if (database == null)
                return;

            Console.WriteLine(ex.ToFormattedString());
            PgnWriter pgnWriter = new PgnWriter(@$"PGNs\Failures\PGN_Failures_{DateTime.Now.ToFileName()}_{Guid.NewGuid()}.pgn");
            pgnWriter.Write(database);
        }
    }

    private static void ProcessGame(Game game)
    {
        IPosition position = new Position();

        Dictionary<string, List<MoveTextEntry>> moves = game.MoveText
            .GroupBy(i => i.GetType().Name)
            .ToDictionary(k => k.Key, k => k.ToList());

        if (moves.ContainsKey(nameof(MovePairEntry)))
        {
            foreach (MovePairEntry entry in moves[nameof(MovePairEntry)])
            {
                if (position.GetHistory().Count() > _depth) break;

                ProcessWhiteMove(position, entry.White);

                ProcessBlackMove(position, entry.Black);
            }

            if (position.GetHistory().Count() <= _depth && moves.TryGetValue(nameof(HalfMoveEntry), out var halfMoveEntry))
            {
                HalfMoveEntry entry = halfMoveEntry.FirstOrDefault() as HalfMoveEntry;

                ProcessWhiteMove(position, entry.Move);
            }

            ProcessEndGame(moves);
        }
        else if (moves.ContainsKey(nameof(HalfMoveEntry)))
        {
            foreach (HalfMoveEntry entry in moves[nameof(HalfMoveEntry)])
            {
                if (position.GetHistory().Count() > _depth) break;

                if (!entry.IsContinued)
                {
                    ProcessWhiteMove(position, entry.Move);
                }

                else
                {
                    ProcessBlackMove(position, entry.Move);
                }
            }

            ProcessEndGame(moves);
        }
    }

    private static void ProcessEndGame(Dictionary<string, List<MoveTextEntry>> moves)
    {
        if (moves.TryGetValue(nameof(GameEndEntry), out var resultEntry))
        {
            GameEndEntry entry = resultEntry.FirstOrDefault() as GameEndEntry;

            var das = Boot.GetService<IDataAccessService>();
            try
            {
                das.Connect();

                if (entry.Result == GameResult.White)
                {
                    das.UpdateHistory(Engine.Book.Models.GameValue.WhiteWin);
                }
                else if (entry.Result == GameResult.Black)
                {
                    das.UpdateHistory(Engine.Book.Models.GameValue.BlackWin);
                }
                else
                {
                    das.UpdateHistory(Engine.Book.Models.GameValue.Draw);
                }
            }
            finally
            {
                das.Disconnect();
            }
        }
        else
        {

        }
    }

    private static void ProcessBlackMove(
        IPosition position, OpeningMentor.Chess.Model.Move entry)
    {
        List<MoveBase> moves = null;

        if (entry.TargetSquare == null)
        {
            if (entry.Type == MoveType.CastleKingSide)
            {
                moves = position.GetMoves(Pieces.BlackKing, Squares.G8);
            }
            else if (entry.Type == MoveType.CastleQueenSide)
            {
                moves = position.GetMoves(Pieces.BlackKing, Squares.C8);
            }
            else
            {

            }
        }
        else
        {
            var squareString = entry.TargetSquare.ToString();
            var square = _squares[squareString];
            var pieceString = $"Black{entry.Piece}";
            var piece = _pieces[pieceString];
            moves = position.GetMoves(piece, square);
        }

        if (moves.Count == 0)
        {
            throw new ArgumentException($"Move list is empty try to find {entry}");
        }
        else if (moves.Count == 1)
        {
            position.Make(moves[0]);
        }
        else
        {
            if (entry.OriginSquare != null)
            {
                string squareString = entry.OriginSquare.ToString();
                var square = _squares[squareString];
                var m = moves.FirstOrDefault(d => d.From == square);
                position.Make(m);
            }
            else if (entry.OriginFile != null)
            {
                var m = moves.FirstOrDefault(m => m.From.AsString().StartsWith(entry.OriginFile.ToString()));
                position.Make(m);
            }
            else if (entry.OriginRank != null)
            {
                var m = moves.FirstOrDefault(m => m.From.AsString().EndsWith(entry.OriginRank.ToString()));
                position.Make(m);
            }
            else if (entry.PromotedPiece != null)
            {
                var pieceString = $"Black{entry.PromotedPiece}";
                var piece = _pieces[pieceString];
                var m = moves.OfType<PromotionMove>().FirstOrDefault(m => m.PromotionPiece == piece);
                position.Make(m);
            }
            else
            {

            }
        }
    }

    private static void ProcessWhiteMove(
        IPosition position, OpeningMentor.Chess.Model.Move entry)
    {
        List<MoveBase> moves = null;
        if (entry.TargetSquare == null)
        {
            if (entry.Type == MoveType.CastleKingSide)
            {
                moves = position.GetMoves(Pieces.WhiteKing, Squares.G1);
            }
            else if (entry.Type == MoveType.CastleQueenSide)
            {
                moves = position.GetMoves(Pieces.WhiteKing, Squares.C1);
            }
            else
            {

            }
        }
        else
        {
            string squareString = entry.TargetSquare.ToString();
            var square = _squares[squareString];
            var pieceString = $"White{entry.Piece}";
            var piece = _pieces[pieceString];
            moves = position.GetMoves(piece, square);
        }

        if (moves.Count == 0)
        {
            throw new ArgumentException($"Move list is empty try to find {entry}");
        }
        else if (moves.Count == 1)
        {
            if (position.GetHistory().Any())
                position.Make(moves[0]);
            else
                position.MakeFirst(moves[0]);
        }
        else
        {
            if (entry.OriginSquare != null)
            {
                string squareString = entry.OriginSquare.ToString();
                var square = _squares[squareString];
                var m = moves.FirstOrDefault(d => d.From == square); 
                position.Make(m);
            }
            else if (entry.OriginFile != null)
            {
                var m = moves.FirstOrDefault(m => m.From.AsString().StartsWith(entry.OriginFile.ToString()));
                position.Make(m);
            }
            else if (entry.OriginRank != null)
            {
                var m = moves.FirstOrDefault(m => m.From.AsString().EndsWith(entry.OriginRank.ToString()));
                position.Make(m);
            }
            else if (entry.PromotedPiece != null)
            {
                var pieceString = $"White{entry.PromotedPiece}";
                var piece = _pieces[pieceString];
                var m = moves.OfType<PromotionMove>().FirstOrDefault(m => m.PromotionPiece == piece);
                position.Make(m);
            }
            else
            {

            }
        }
    }
}