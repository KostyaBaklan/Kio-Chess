using Engine.Models.Moves;
using System.Text;

namespace StockfishApp.Models
{
    internal class FullMoves
    {
        public FullMoves() 
        {
            Moves = new List<FullMove>(); 
        }

        public List<FullMove> Moves { get; set; }

        public void Add(MoveBase move)
        {
            if (move.IsWhite)
            {
                Moves.Add(new FullMove { White= move });
            }
            else
            {
                Moves.Last().Black= move;
            }
        }

        public override string ToString()
        {
            StringBuilder list = new StringBuilder();

            for (int i = 0; i < Moves.Count; i++)
            {
                FullMove move = Moves[i];
                list.AppendLine(move.ToString(i+1));
            }

            return list.ToString();
        }
    }
    internal class FullMove
    {
        public MoveBase White { get; set; }
        public MoveBase Black { get; set; }

        public string ToString(int index)
        {
            if(Black == null) return $"{index}. {White.ToLightString()}";

            return $"{index}. {White.ToLightString()} {Black.ToLightString()}";
        }
    }
}
