using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using GamesServices;
using OpeningMentor.Chess.Model;
using OpeningMentor.Chess.Model.MoveText;
using OpeningMentor.Chess.Pgn;
using Tools.Common;
using GameResult = OpeningMentor.Chess.Model.MoveText.GameResult;

internal class Program
{
    private static int _depth;
    private static Dictionary<string, byte> _squares = new Dictionary<string, byte>();
    private static Dictionary<string, byte> _pieces = new Dictionary<string, byte>();
    private static ISequenceService _service;

    private static void Main(string[] args)
    {
        Boot.SetUp();

        SequenceClient client = new SequenceClient();
        _service = client.GetService();

        try
        {
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

            var dir = @"C:\Projects\AI\Kio-Chess\KioChess\Data\Release\net7.0\PGNs\Failures";

            var file = Path.Combine(dir, "PGN_Failures_2023_09_20_02_38_24_2431_37225d10-2a19-4c2a-8712-0a38596072e6.pgn");

            //ProcessFile(file);

            ProcessArgument(args);
        }
        finally
        {
            client.Close();
        }
    }

    private static void ProcessFile(string fileName)
    {
        FileInfo file = new FileInfo(fileName);
        try
        {
            PgnReader pgnReader = new PgnReader();
            Database database = pgnReader.ReadFromFile(file.FullName);

            var game = database.Games.FirstOrDefault();

            ProcessGame(game);

            Console.WriteLine("No failure !!!");

            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to delete '{file.FullName}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(file.FullName);
            Console.WriteLine(ex.ToFormattedString());
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

        var ms = game.MoveText.GetMoves().Take(_depth).ToList();
        var end = game.MoveText.FirstOrDefault(m => m.Type == MoveTextEntryType.GameEnd) as GameEndEntry;

        if(end== null)
        {
            string result;
            if(!game.Tags.TryGetValue("Result", out result) )
            {
                var ai = game.AdditionalInfo.FirstOrDefault(a => a.Name == "Result");
                if (ai != null)
                {
                    result = ai.Value.ToString();
                }
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.ToLower();
                if (result == "1 - 0" || result == "1-0" || result == "white")
                {
                    end = new GameEndEntry(GameResult.White);
                }
                else if (result == "0 - 1" || result == "0-1" || result == "black")
                {
                    end = new GameEndEntry(GameResult.Black);
                }
                else if (result == "1/2 - 1/2" || result == "0.5 - 0.5" || result == "1/2-1/2" || result == "0.5-0.5" || result == "draw")
                {
                    end = new GameEndEntry(GameResult.Draw);
                } 
            }
        }

        if (ms != null && end != null && ms.Any())
        {
            bool isWhite = true;
            foreach (var m in ms)
            {
                if (isWhite)
                {
                    isWhite = false;
                    ProcessWhiteMove(position, m);
                }
                else
                {
                    isWhite = true;
                    ProcessBlackMove(position, m);
                }
            }

            ProcessEndGame(end);
        }
        else
        {
            throw new Exception("No Moves or End Game!"); 
        }
    }

    private static void ProcessMoveText(Game game, IPosition position)
    {
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

    private static void ProcessEndGame(GameEndEntry entry)
    {
        var das = Boot.GetService<IGameDbService>();

        List<HistoryRecord> records = new List<HistoryRecord>();
        try
        {
            das.Connect();

            if (entry.Result == GameResult.White)
            {
                records = das.CreateRecords(1,0,0);
            }
            else if (entry.Result == GameResult.Black)
            {
                records = das.CreateRecords(0, 0, 1);
            }
            else
            {
                records = das.CreateRecords(0, 1, 0);
            }

            var models = records.Select(s=>new SequenceModel
            {
                Sequence = s.Sequence,
                Move = s.Move,
                White = s.White,
                Black = s.Black,
                Draw = s.Draw
            }).ToList();

            _service.ProcessSequence(models);
        }
        finally
        {
            das.Disconnect();
        }
    }

    private static void ProcessEndGame(Dictionary<string, List<MoveTextEntry>> moves)
    {
        if (moves.TryGetValue(nameof(GameEndEntry), out var resultEntry))
        {
            GameEndEntry entry = resultEntry.FirstOrDefault() as GameEndEntry;

            ProcessEndGame(entry);
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