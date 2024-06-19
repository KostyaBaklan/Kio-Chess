using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;

namespace Tools.Common;

public class MoveSequenceParser
{
    private Dictionary<string, byte> _squares = new Dictionary<string, byte>();
    private Dictionary<string, byte> _pieces = new Dictionary<string, byte>();
    private Dictionary<string, string> _subPieces = new Dictionary<string, string>();

    private readonly Position _position;
    private readonly MoveHistoryService _moveHistoryService;

    public MoveSequenceParser(Position position, MoveHistoryService moveHistoryService)
    {
        _position = position;
        _moveHistoryService = moveHistoryService;

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

        _subPieces = new Dictionary<string, string>
        {
            {"N","Knight" },{"n","Knight" },
            {"B","Bishop" },{"b","Bishop" },
            {"R","Rook" },{"r","Rook" },
            {"Q","Queen" },{"q","Queen" },
            {"K","King" },{"k","King" }
        };
    }

    public short[] ParseAsKeys(string[] moves)
    {
        bool isValid = true;
        short[] key = null;

        int j = 0;

        for (int i = 0; i < moves.Length; i++)
        {
            string m = moves[i].TrimEnd(new char[] { '+', '#' });
            MoveBase move;

            if (i % 2 == 0)
            {
                move = ParseWhiteMove(m);
            }
            else
            {
                move = ParseBlackMove(m);
            }

            if (move == null)
            {
                isValid = false;
                break;
            }

            if (i != 0)
                _position.Make(move);
            else
                _position.MakeFirst(move);
            j++;
        }

        if (isValid)
        {
            key = GetKeys();
        }

        for (int i = 0; i < j; i++)
        {
            _position.UnMake();
        }

        return key.Take(j).ToArray();
    }

    private short[] GetKeys() => _moveHistoryService.GetKeys();

    public string Parse(string[] moves)
    {
        bool isValid = true;
        string key = string.Empty;

        int j = 0;

        for (int i = 0; i < moves.Length; i++)
        {
            string m = moves[i].TrimEnd(new char[] { '+', '#' });
            MoveBase move;

            if (i % 2 == 0)
            {
                move = ParseWhiteMove(m);
            }
            else
            {
                move = ParseBlackMove(m);
            }

            if (move == null)
            {
                isValid = false;
                break;
            }

            if (i != 0)
                _position.Make(move);
            else
                _position.MakeFirst(move);
            j++;
        }

        if (isValid)
        {
            key = GetKey(j);
        }

        for (int i = 0; i < j; i++)
        {
            _position.UnMake();
        }

        return key;
    }

    public bool IsValid(List<string> moves)
    {
        bool isValid = true;

        int j = 0;

        for (int i = 0; i < moves.Count; i++)
        {
            string m = moves[i].TrimEnd(new char[] { '+', '#' });
            MoveBase move;

            if (i % 2 == 0)
            {
                move = ParseWhiteMove(m);
            }
            else
            {
                move = ParseBlackMove(m);
            }

            if (move == null)
            {
                isValid = false;
                break;
            }

            if (i != 0)
                _position.Make(move);
            else
                _position.MakeFirst(move);
            j++;
        }

        for (int i = 0; i < j; i++)
        {
            _position.UnMake();
        }

        return isValid;
    }

    private string GetKey() => _moveHistoryService.GetSequenceKey();

    private string GetKey(int length) => _moveHistoryService.GetSequenceKey(length);

    private MoveBase ParseWhiteMove(string m)
    {
        try
        {
            string squareString = null;
            string pieceString = null;
            char from = '!';
            if (m == "0-0"|| m == "o-o"|| m == "O-O")
            {
                squareString = "g1";
                pieceString = "WhiteKing";
            }
            else if (m == "0-0-0" || m == "o-o-o" || m == "O-O-O")
            {
                squareString = "c1";
                pieceString = "WhiteKing";
            }
            else if (m.Length == 2)
            {
                squareString = m;
                pieceString = "WhitePawn";
            }
            else if (m.Length == 3)
            {
                squareString = m.Substring(1);
                var p = m.Substring(0, 1);
                pieceString = $"White{_subPieces[p]}";
            }
            else if (m.Length == 4)
            {
                if (m.Contains("x"))
                {
                    var parts = m.Split('x');
                    squareString = parts[1];
                    if (parts[0] != "b" && _subPieces.TryGetValue(parts[0], out var p))
                    {
                        pieceString = $"White{p}";
                    }
                    else
                    {
                        pieceString = $"WhitePawn";
                        from = parts[0][0];
                    }
                }
                else
                {
                    var parts = m.Split(m[1]);
                    squareString = parts[1];
                    pieceString = $"White{_subPieces[parts[0]]}";
                    from = m[1];
                }
            }
            else if (m.Length == 5)
            {
                squareString = m;
                pieceString = $"WhitePawn";
            }
            else
            {

            }
            var square = _squares[squareString];
            var piece = _pieces[pieceString];
            var moves = _position.GetMoves(piece, square);

            if (moves == null)
            {
                return null;
            }
            if (moves.Count == 1)
            {
                return moves[0];
            }
            if (moves.Count == 2 && from!='!')
            {
                return moves.FirstOrDefault(mo=>mo.From.AsString().ToLower().Contains(from));
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during parse of {m}");
            Console.WriteLine(ex.ToFormattedString());
            return null;
        }
    }

    private MoveBase ParseBlackMove(string m)
    {
        try
        {
            char from = '!';
            string squareString = null;
            string pieceString = null;
            if (m == "0-0" || m == "o-o" || m == "O-O")
            {
                squareString = "g8";
                pieceString = "BlackKing";
            }
            else if (m == "0-0-0" || m == "o-o-o" || m == "O-O-O")
            {
                squareString = "c8";
                pieceString = "BlackKing";
            }
            else if (m.Length == 2)
            {
                squareString = m;
                pieceString = "BlackPawn";
            }
            else if (m.Length == 3)
            {
                squareString = m.Substring(1);
                var p = m.Substring(0, 1);
                pieceString = $"Black{_subPieces[p]}";
            }
            else if (m.Length == 4)
            {
                if (m.Contains("x"))
                {
                    var parts = m.Split('x');
                    squareString = parts[1];
                    if (parts[0]!="b" && _subPieces.TryGetValue(parts[0], out var p))
                    {
                        pieceString = $"Black{p}";
                    }
                    else
                    {
                        pieceString = $"BlackPawn";
                        from = parts[0][0];
                    }
                }
                else
                {
                    var parts = m.Split(m[1]);
                    squareString = parts[1];
                    pieceString = $"Black{_subPieces[parts[0]]}";
                    from = m[1];
                }
            }
            else if (m.Length == 5)
            {
                squareString = m;
                pieceString = $"BlackPawn";
            }
            else
            {

            }
            var square = _squares[squareString];
            var piece = _pieces[pieceString];
            var moves = _position.GetMoves(piece, square);

            if (moves == null)
            {
                return null;
            }
            if (moves.Count == 1)
            {
                return moves[0];
            }
            if (moves.Count == 2 && from != '!')
            {
                return moves.FirstOrDefault(mo => mo.From.AsString().ToLower().Contains(from));
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during parse of {m}");
            Console.WriteLine(ex.ToFormattedString());
            return null;
        }
    }

    public MoveBase Parse(string bestMove, bool isWhite)
    {
        if(isWhite)
        {
            return ParseWhiteMove(bestMove);
        }
        else
        {
            return ParseBlackMove(bestMove);
        }
    }
}
