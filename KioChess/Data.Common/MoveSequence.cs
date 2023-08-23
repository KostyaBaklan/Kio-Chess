using Engine.Models.Moves;

namespace Data.Common
{
    public class MoveSequence
    {
        public MoveSequence()
        {
            Keys = new List<short>();
            Moves = new List<string>();
        }

        public MoveSequence(MoveSequence ms)
        {
            Keys = new List<short>(ms.Keys);
            Moves = new List<string>(ms.Moves);
        }

        public List<short> Keys { get; set; }
        public List<string> Moves { get; set; }

        public void Add(MoveBase move)
        {
            Keys.Add(move.Key);
            Moves.Add(move.ToLightString());
        }

        public override string ToString()
        {
            return string.Join(" -> ", Moves);
        }
    }
}