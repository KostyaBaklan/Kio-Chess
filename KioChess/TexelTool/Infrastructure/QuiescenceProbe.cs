using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;

// Add the same using directives StrategyBase.cs has for NullStrategy and StrategyType if they don't resolve.
namespace TexelTool
{
    // Uses the engine's quiescence search (captures + promotions, SEE-ordered, delta-pruned) to test whether a position is quiet.
    internal sealed class QuiescenceProbe : StrategyBase
    {
        public QuiescenceProbe(int depth, Position position) : base(depth, position)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetComplex(position));
        }
        public override StrategyType Type => default;
        protected override StrategyBase CreateEndGameStrategy() => null;
        protected override NullStrategy CreateNullStrategy() => null;
        // qsearch value minus stand-pat for the side to move (>= 0). int.MaxValue when in check.
        public int Gain()
        {
            if (MoveHistory.IsLastMoveWasCheck()) return int.MaxValue;
            DataPoolService.Resize(Table);
            bool white = Position.GetTurn() == Turn.White;
            int standPat = white ? _board.Evaluate() : _board.EvaluateOpposite();
            int q = white ? EvaluateWhite(MinusSearchValue, SearchValue) : EvaluateBlack(MinusSearchValue, SearchValue);
            return q - standPat;
        }
    }
}