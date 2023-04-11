using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Tools.Common
{
    public static  class GameProvider
    {
        private static Dictionary<string, List<MoveBase>> _movesDictionary;

        static GameProvider()
        {
            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            _movesDictionary = new Dictionary<string, List<MoveBase>>
            {
                {
                    "king", new List<MoveBase>
                    {
                        moveProvider.GetMoves(Pieces.WhitePawn, Squares.E2).FirstOrDefault(m => m.To == Squares.E4),
                        moveProvider.GetMoves(Pieces.BlackPawn, Squares.E7).FirstOrDefault(m => m.To == Squares.E5)
                    }
                },
                {
                    "queen", new List<MoveBase>
                    {
                        moveProvider.GetMoves(Pieces.WhitePawn, Squares.D2).FirstOrDefault(m => m.To == Squares.D4),
                        moveProvider.GetMoves(Pieces.BlackKnight, Squares.G8).FirstOrDefault(m => m.To == Squares.F6)
                    }
                },
                {
                    "sicilian", new List<MoveBase>
                    {
                        moveProvider.GetMoves(Pieces.WhitePawn, Squares.E2).FirstOrDefault(m => m.To == Squares.E4),
                        moveProvider.GetMoves(Pieces.BlackPawn, Squares.C7).FirstOrDefault(m => m.To == Squares.C5)
                    }
                },
                {
                    "english", new List<MoveBase>
                    {
                        moveProvider.GetMoves(Pieces.WhitePawn, Squares.C2).FirstOrDefault(m => m.To == Squares.C4),
                        moveProvider.GetMoves(Pieces.BlackPawn, Squares.E7).FirstOrDefault(m => m.To == Squares.E5)
                    }
                }
            };
        }

        public static List<MoveBase> GetMoves(string game)
        {
            return _movesDictionary[game];
        }
    }
}
