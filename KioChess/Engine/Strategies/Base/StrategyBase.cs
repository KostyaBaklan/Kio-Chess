using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using Engine.Strategies.AB;
using Engine.Strategies.End;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Base
{
    public abstract partial class StrategyBase
    {
        private bool _isBlocked;
        protected bool UseAging;
        protected short Depth;
        protected int SearchValue;
        protected int ThreefoldRepetitionValue;
        protected int FutilityDepth;
        protected bool UseFutility;
        protected bool UseSortHard;
        protected bool UseSortDifference;
        protected int MaxEndGameDepth;

        //protected SearchContext CurrentSearchContext;

        protected int[] SortDepth;
        protected int[] SortHardDepth;
        protected int[] SortDifferenceDepth;
        protected int[][] FutilityMargins;

        protected int SubSearchDepthThreshold;
        protected int SubSearchDepth;
        protected int SubSearchLevel;
        protected bool UseSubSearch;

        protected IPosition Position;
        protected IMoveSorter[] Sorters;

        protected IEvaluationService EvaluationService;
        protected readonly IMoveHistoryService MoveHistory;
        protected readonly IMoveProvider MoveProvider;
        protected readonly IMoveSorterProvider MoveSorterProvider;
        protected readonly IConfigurationProvider configurationProvider;

        private StrategyBase _endGameStrategy;
        protected StrategyBase EndGameStrategy
        {
            get { return _endGameStrategy ??= CreateEndGameStrategy(); }
        }

        private StrategyBase _subSearchStrategy;

        protected StrategyBase SubSearchStrategy
        {
            get
            {
                return _subSearchStrategy ??= CreateSubSearchStrategy();
            }
        }

        protected StrategyBase(short depth, IPosition position)
        {
            configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            var algorithmConfiguration = configurationProvider.AlgorithmConfiguration;
            var sortingConfiguration = algorithmConfiguration.SortingConfiguration;
            var generalConfiguration = configurationProvider.GeneralConfiguration;

            MaxEndGameDepth = algorithmConfiguration.MaxEndGameDepth;
            SortDepth = sortingConfiguration.SortDepth;
            SortHardDepth = sortingConfiguration.SortHardDepth;
            SortDifferenceDepth = sortingConfiguration.SortDifferenceDepth;
            SearchValue = configurationProvider.Evaluation.Static.Mate;
            ThreefoldRepetitionValue = configurationProvider.Evaluation.Static.ThreefoldRepetitionValue;
            UseFutility = generalConfiguration.UseFutility;
            FutilityDepth = generalConfiguration.FutilityDepth;
            UseAging = generalConfiguration.UseAging;
            UseSortHard = sortingConfiguration.UseSortHard;
            UseSortDifference = sortingConfiguration.UseSortDifference;
            Depth = depth;
            Position = position;

            //InitializeSearchContext();

            SubSearchDepthThreshold = configurationProvider
                    .AlgorithmConfiguration.SubSearchConfiguration.SubSearchDepthThreshold;
            SubSearchDepth = configurationProvider
                    .AlgorithmConfiguration.SubSearchConfiguration.SubSearchDepth;
            SubSearchLevel = configurationProvider
                    .AlgorithmConfiguration.SubSearchConfiguration.SubSearchLevel;
            UseSubSearch = configurationProvider
                    .AlgorithmConfiguration.SubSearchConfiguration.UseSubSearch;

            EvaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            MoveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            MoveSorterProvider = ServiceLocator.Current.GetInstance<IMoveSorterProvider>();

            InitializeMargins();
        }

        public virtual int Size => 0;

        public virtual IResult GetResult()
        {
            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.GetResult();
            }
            return GetResult(-SearchValue, SearchValue, Depth);
        }

        public virtual IResult GetResult(int alpha, int beta, int depth, MoveBase pv = null)
        {
            Result result = new Result();
            if (IsDraw(result))
            {
                return result;
            }

            var moves = Position.GetAllMoves(Sorters[Depth], pv);

            if (CheckEndGame(moves.Length, result)) return result;

            if (moves.Length > 1)
            {
                SetResult(alpha, beta, depth, result, moves);
            }
            else
            {
                result.Move = moves[0];
            }

            result.Move.History++;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual int Search(int alpha, int beta, int depth)
        {
            if (depth <= 0) return Evaluate(alpha, beta);

            if (Position.GetPhase() == Phase.End)
                return EndGameStrategy.Search(alpha, beta, depth);

            if (CheckDraw()) return 0;

            SearchContext context = GetCurrentContext(alpha, depth);

            if (!context.IsEndGame)
            {
                if (context.IsFutility)
                {
                    FutilitySearchInternal(alpha, beta, depth, context);
                }
                else
                {
                    SearchInternal(alpha, beta, depth, context);
                }
            }

            return context.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void FutilitySearchInternal(int alpha, int beta, int depth, SearchContext context)
        {
            MoveBase move;
            int r;
            int d = depth - 1;
            int b = -beta;

            for (var i = 0; i < context.Moves.Length; i++)
            {
                move = context.Moves[i];

                Position.Make(move);

                if (!move.IsCheck && move.IsFutile)
                {
                    Position.UnMake();
                    continue;
                }

                r = -Search(b, -alpha, d);

                Position.UnMake();

                if (r > context.Value)
                {
                    context.Value = r;
                    context.BestMove = move;

                    if (context.Value >= beta)
                    {
                        if (!move.IsAttack) Sorters[depth].Add(move.Key);
                        break;
                    }
                    else
                    {
                        if (context.Value > alpha)
                        {
                            alpha = context.Value;
                        }
                    }
                }
            }

            if (context.Value == int.MinValue)
            {
                context.IsEndGame = true;
            }
            else
            {
                context.BestMove.History += 1 << depth;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void SearchInternal(int alpha, int beta, int depth, SearchContext context)
        {
            MoveBase move;
            int r;
            int d = depth - 1;
            int b = -beta;

            for (var i = 0; i < context.Moves.Length; i++)
            {
                move = context.Moves[i];

                Position.Make(move);

                r = -Search(b, -alpha, d);

                Position.UnMake();

                if (r > context.Value)
                {
                    context.Value = r;
                    context.BestMove = move;

                    if (context.Value >= beta)
                    {
                        if (!move.IsAttack) Sorters[depth].Add(move.Key);
                        break;
                    }
                    else
                    {
                        if (context.Value > alpha)
                        {
                            alpha = context.Value;
                        }
                    }
                }
            }

            context.BestMove.History += 1 << depth;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetResult(int alpha, int beta, int depth, Result result, MoveBase[] moves)
        {
            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                Position.Make(move);

                var value = -Search(-beta, -alpha, depth - 1);

                Position.UnMake();
                if (value > result.Value)
                {
                    result.Value = value;
                    result.Move = move;
                }

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;
                break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual SearchContext GetCurrentContext(int alpha, int depth, MoveBase pv = null)
        {
            SearchContext context = new SearchContext
            {
                IsFutility = IsFutility(alpha, depth),

                Moves = Position.GetAllMoves(Sorters[depth], pv)
            };

            if (CheckEndPosition(context.Moves.Length, out int endGameValue))
            {
                context.IsEndGame = true;
                context.Value = endGameValue;
            }
            else
            {
                context.IsEndGame = false;
            }

            return context;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public void Forward()
        //{
        //    //CurrentSearchContext = CurrentSearchContext.Next;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public void Back()
        //{
        //    //CurrentSearchContext = CurrentSearchContext.Previous;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MoveBase[] SubSearch(MoveBase[] moves, int alpha, int beta, int depth)
        {
            if (UseSubSearch && Depth - depth < SubSearchLevel && depth - SubSearchDepth > SubSearchDepthThreshold)
            {
                ValueMove[] valueMoves = new ValueMove[moves.Length];
                for (var i = 0; i < moves.Length; i++)
                {
                    Position.Make(moves[i]);

                    valueMoves[i] = new ValueMove { Move = moves[i], Value = -SubSearchStrategy.Search(-beta, -alpha, depth - SubSearchDepth) };

                    Position.UnMake();
                }

                Array.Sort(valueMoves);

                for (int i = 0; i < valueMoves.Length; i++)
                {
                    moves[i] = valueMoves[i].Move;
                }
            }

            return moves;
        }
        protected virtual StrategyBase CreateSubSearchStrategy()
        {
            return new NegaMaxMemoryStrategy((short)(Depth - SubSearchDepth), Position);
        }

        protected virtual StrategyBase CreateEndGameStrategy()
        {
            return new LmrNoCacheStrategy((short)Math.Min(Depth + 1, MaxEndGameDepth), Position);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsFutility(int alpha, int depth)
        {
            if (depth > FutilityDepth||MoveHistory.IsLastMoveWasCheck()) return false;

            return Position.GetStaticValue() + FutilityMargins[(byte)Position.GetPhase()][depth - 1] <= alpha;
        }
        protected virtual void InitializeSorters(short depth, IPosition position, MoveSorter mainSorter)
        {
            Sorters = new IMoveSorter[depth + 2];

            var initialSorter = MoveSorterProvider.GetInitial(position, Sorting.Sort.HistoryComparer);
            Sorters[0] = MoveSorterProvider.GetBasic(position, Sorting.Sort.HistoryComparer);

            var d = depth - SortDepth[depth];

            if (UseSortHard)
            {
                var hardExtended = MoveSorterProvider.GetHardExtended(position, Sorting.Sort.HistoryComparer);
                var hard = d - SortHardDepth[depth];
                for (int i = 1; i < hard; i++)
                {
                    Sorters[i] = mainSorter;
                }

                for (var i = hard; i < d; i++)
                {
                    Sorters[i] = hardExtended;
                }
            }
            else if (UseSortDifference)
            {
                var differenceExtended =
                    MoveSorterProvider.GetDifferenceExtended(position, Sorting.Sort.HistoryComparer);
                var x = 1 + SortDifferenceDepth[depth];
                for (int i = 1; i < x; i++)
                {
                    Sorters[i] = differenceExtended;
                }

                for (int i = x; i < d; i++)
                {
                    Sorters[i] = mainSorter;
                }
            }
            else
            {
                for (int i = 1; i < d; i++)
                {
                    Sorters[i] = mainSorter;
                }
            }


            for (var i = d; i < Sorters.Length; i++)
            {
                Sorters[i] = initialSorter;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int Evaluate(int alpha, int beta)
        {
            int standPat = Position.GetValue();
            if (standPat >= beta)
            {
                return beta;
            }

            if (alpha < standPat)
                alpha = standPat;

            var moves = Position.GetAllAttacks(Sorters[0]);
            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                Position.Make(move);

                int score = -Evaluate(-beta, -alpha);

                Position.UnMake();

                if (score >= beta)
                {
                    return beta;
                }

                if (score > alpha)
                    alpha = score;
            }

            return alpha;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsEndGameDraw(Result result)
        {
            if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
            {
                result.GameResult = GameResult.ThreefoldRepetition;
                result.Value = 0;
                return true;
            }

            if (MoveHistory.IsFiftyMoves())
            {
                result.GameResult = GameResult.FiftyMoves;
                result.Value = 0;
                return true;
            }

            if (Position.IsDraw())
            {
                result.GameResult = GameResult.Draw;
                result.Value = 0;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsDraw(Result result)
        {
            if (Position.GetPhase() == Phase.Opening) return false;

            if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
            {
                result.GameResult = GameResult.ThreefoldRepetition;
                result.Value = 0;
                return true;
            }

            if (Position.GetPhase() == Phase.Middle) return false;

            if (MoveHistory.IsFiftyMoves())
            {
                result.GameResult = GameResult.FiftyMoves;
                result.Value = 0;
                return true;
            }

            if (Position.IsDraw())
            {
                result.GameResult = GameResult.Draw;
                result.Value = 0;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckEndGame(int count, Result result)
        {
            if (count > 0)
            {
                return false;
            }

            if (MoveHistory.IsLastMoveWasCheck())
            {
                result.GameResult = GameResult.Mate;
                result.Value = EvaluationService.GetMateValue();
            }
            else
            {
                result.GameResult = GameResult.Pat;
                result.Value = 0;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckEndGameDraw()
        {
            return MoveHistory.IsThreefoldRepetition(Position.GetKey()) || MoveHistory.IsFiftyMoves() || Position.IsDraw();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckDraw()
        {
            if (Position.GetPhase() == Phase.Opening) return false;

            if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
            {
                return true;
            }

            if (Position.GetPhase() == Phase.Middle) return false;

            return MoveHistory.IsFiftyMoves() || Position.IsDraw();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckEndPosition(int count, out int value)
        {
            value = 0;
            if (count == 0)
            {
                if (MoveHistory.IsLastMoveWasCheck())
                {
                    value = -EvaluationService.GetMateValue();
                }
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{GetType().Name}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsBlocked()
        {
            return _isBlocked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void ExecuteAsyncAction()
        {
            _isBlocked = true;
            Task.Factory.StartNew(() => { _isBlocked = false; });
        }

        private void InitializeMargins()
        {
            FutilityMargins = new int[3][];
            FutilityMargins[0] = new[]
            {
                EvaluationService.GetValue(2, Phase.Opening),
                EvaluationService.GetValue(3, Phase.Opening)+EvaluationService.GetValue(0, Phase.Opening),
                EvaluationService.GetValue(4, Phase.Opening)
            };
            FutilityMargins[1] = new[]
            {
                EvaluationService.GetValue(2, Phase.Middle),
                EvaluationService.GetValue(3, Phase.Middle)+EvaluationService.GetValue(0, Phase.Middle),
                EvaluationService.GetValue(4, Phase.Middle)
            };
            FutilityMargins[2] = new[]
            {
                EvaluationService.GetValue(2, Phase.End),
                EvaluationService.GetValue(3, Phase.End)+EvaluationService.GetValue(0, Phase.End),
                EvaluationService.GetValue(4, Phase.End)
            };
        }

        //private void InitializeSearchContext()
        //{
        //    SearchContext[] contexts = new SearchContext[configurationProvider.GeneralConfiguration.GameDepth];
        //    for (int i = 0; i < configurationProvider.GeneralConfiguration.GameDepth; i++)
        //    {
        //        contexts[i] = new SearchContext() { Ply = i };
        //    }
        //    for (int i = 1; i < configurationProvider.GeneralConfiguration.GameDepth - 1; i++)
        //    {
        //        contexts[i - 1].Next = contexts[i];
        //        contexts[i].Previous = contexts[i - 1];
        //        contexts[i].Next = contexts[i + 1];
        //        contexts[i + 1].Previous = contexts[i];
        //    }
        //    CurrentSearchContext = contexts[0];
        //}
    }
}
