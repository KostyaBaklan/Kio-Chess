using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using OpeningMentor.Chess.Model;
using OpeningMentor.Chess.Model.Basic;
using OpeningMentor.Chess.Model.MoveText;
using OpeningMentor.Chess.Pgn;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();

        PgnReader pgnReader = new PgnReader();

        Dictionary<string, byte> squares = new Dictionary<string, byte>();

        for (byte i = 0; i < 64; i++)
        {
            var k = i.AsString().ToLower();
            squares[k] = i;
        }

        Dictionary<string, byte> pieces = new Dictionary<string, byte>();

        for (byte  i = 0; i < 12; i++)
        {
            var p = i.AsEnumString();
            pieces[p] = i;
        }

        IEnumerable<Game> games = pgnReader.ReadGamesFromFile(@"C:\Dev\Temp\PgnTest.pgn");

        foreach (Game game in games)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();

            IPosition position = new Position();

            Dictionary<string, List<MoveTextEntry>> moves = game.MoveText
                .GroupBy(i => i.GetType().Name)
                .ToDictionary(k => k.Key, k => k.ToList());
            
            foreach (MovePairEntry entry in moves[nameof(MovePairEntry)])
            {
                var white = entry.White;

                List<MoveBase> whiteMoves = null;
                if (white.TargetSquare == null)
                {
                    if(white.Type == MoveType.CastleKingSide)
                    {
                        whiteMoves = position.GetMoves(Pieces.WhiteKing, Squares.G1);
                    }
                    else if(white.Type == MoveType.CastleQueenSide)
                    {
                        whiteMoves = position.GetMoves(Pieces.WhiteKing, Squares.C1);
                    }
                    else
                    {

                    }
                }
                else
                {
                    string whiteSquareString = white.TargetSquare.ToString();
                    var whiteSquare = squares[whiteSquareString];
                    var whitePieceString = $"White{white.Piece}";
                    var whitePiece = pieces[whitePieceString]; 
                    whiteMoves = position.GetMoves(whitePiece, whiteSquare);
                }

                if(whiteMoves.Count == 0)
                {

                }
                else if(whiteMoves.Count == 1)
                {
                    if(position.GetHistory().Any())
                        position.Make(whiteMoves[0]);
                    else
                        position.MakeFirst(whiteMoves[0]);

                    list[white.ToString()] = whiteMoves[0].ToString();
                }
                else
                {
                    if (white.OriginFile != null)
                    {
                        var m = whiteMoves.FirstOrDefault(m => m.From.AsString().StartsWith(white.OriginFile.ToString()));
                        position.Make(m);
                        list[white.ToString()] = m.ToString();
                    }
                    else if (white.OriginRank != null)
                    {
                        var m = whiteMoves.FirstOrDefault(m => m.From.AsString().EndsWith(white.OriginRank.ToString()));
                        position.Make(m);
                        list[white.ToString()] = m.ToString();
                    }
                }

                var black = entry.Black; 
                List<MoveBase> blackMoves = null;

                if (black.TargetSquare == null)
                {
                    if (black.Type == MoveType.CastleKingSide)
                    {
                        blackMoves = position.GetMoves(Pieces.BlackKing, Squares.G8);
                    }
                    else if (black.Type == MoveType.CastleQueenSide)
                    {
                        blackMoves = position.GetMoves(Pieces.BlackKing, Squares.C8);
                    }
                    else
                    {

                    }
                }
                else
                {
                    var blackSquareString = black.TargetSquare.ToString();
                    var blackSquare = squares[blackSquareString];
                    var blackPieceString = $"Black{black.Piece}";
                    var blackPiece = pieces[blackPieceString]; 
                    blackMoves = position.GetMoves(blackPiece, blackSquare);
                }

                if (blackMoves.Count == 0)
                {

                }
                else if (blackMoves.Count == 1)
                {
                    position.Make(blackMoves[0]);
                    list[black.ToString()] = blackMoves[0].ToString();
                }
                else
                {
                    if (black.OriginFile != null)
                    {
                        var m = blackMoves.FirstOrDefault(m => m.From.AsString().StartsWith(black.OriginFile.ToString()));
                        position.Make(m);
                        list[white.ToString()] = m.ToString();
                    }
                    else if (black.OriginRank != null)
                    {
                        var m = blackMoves.FirstOrDefault(m => m.From.AsString().EndsWith(black.OriginRank.ToString()));
                        position.Make(m);
                        list[white.ToString()] = m.ToString();
                    }
                }
            }

            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

        Console.WriteLine("Yalla!");
    }
}