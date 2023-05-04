﻿using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
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
        protected sbyte Depth;
        protected short SearchValue;
        protected int ThreefoldRepetitionValue;
        protected int FutilityDepth;
        protected int RazoringDepth;
        protected bool UseFutility;
        protected short MaxEndGameDepth;
        protected int ExtensionDepthDifference;
        protected int EndExtensionDepthDifference;
        protected int DistanceFromRoot;
        protected int MaxExtensionPly;

        protected bool UseComplexSort;
        protected int[] SortDepth;
        protected short[][] FutilityMargins;
        protected short[] DeltaMargins;

        protected int SubSearchDepthThreshold;
        protected int SubSearchDepth;
        protected int SubSearchLevel;
        protected bool UseSubSearch;

        protected const sbyte One = 1;
        protected const sbyte Zero = 0;


        protected readonly MoveBase[] _firstMoves;

        protected IPosition Position;
        protected MoveSorterBase[] Sorters;

        protected IEvaluationService EvaluationService;
        protected readonly IMoveHistoryService MoveHistory;
        protected readonly IMoveProvider MoveProvider;
        protected readonly IMoveSorterProvider MoveSorterProvider;
        protected readonly IConfigurationProvider configurationProvider;
        protected readonly IDataPoolService DataPoolService;

        private StrategyBase _endGameStrategy;
        protected StrategyBase EndGameStrategy
        {
            get 
            { 
                StrategyBase strategyBase = _endGameStrategy ??= CreateEndGameStrategy();
                strategyBase.MaxExtensionPly = MaxExtensionPly - ExtensionDepthDifference + EndExtensionDepthDifference;
                return strategyBase; 
            }
        }

        private StrategyBase _subSearchStrategy;

        protected StrategyBase SubSearchStrategy
        {
            get
            {
                return _subSearchStrategy ??= CreateSubSearchStrategy();
            }
        }

        public static Random Random = new Random();

        protected StrategyBase(short depth, IPosition position)
        {
            configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            var algorithmConfiguration = configurationProvider.AlgorithmConfiguration;
            var sortingConfiguration = algorithmConfiguration.SortingConfiguration;
            var generalConfiguration = configurationProvider.GeneralConfiguration;

            MaxEndGameDepth = configurationProvider.EndGameConfiguration.MaxEndGameDepth;
            SortDepth = sortingConfiguration.SortDepth;
            UseComplexSort = sortingConfiguration.UseComplexSort;
            SearchValue = configurationProvider.Evaluation.Static.Mate;
            ThreefoldRepetitionValue = configurationProvider.Evaluation.Static.ThreefoldRepetitionValue;
            UseFutility = generalConfiguration.UseFutility;
            FutilityDepth = generalConfiguration.FutilityDepth;
            RazoringDepth = FutilityDepth + 1;
            UseAging = generalConfiguration.UseAging;
            Depth = (sbyte)depth;
            Position = position;
            ExtensionDepthDifference = algorithmConfiguration.ExtensionDepthDifference[depth];
            EndExtensionDepthDifference = configurationProvider.AlgorithmConfiguration.EndExtensionDepthDifference[depth];

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
            DataPoolService = ServiceLocator.Current.GetInstance<IDataPoolService>();
            DataPoolService.Initialize(Position);

            InitializeMargins();

            _firstMoves = new MoveBase[]
            {
                MoveProvider.GetMoves(Pieces.WhitePawn,Squares.E2).FirstOrDefault(m=>m.To == Squares.E4),
                MoveProvider.GetMoves(Pieces.WhitePawn,Squares.D2).FirstOrDefault(m=>m.To == Squares.D4),
                MoveProvider.GetMoves(Pieces.WhitePawn,Squares.C2).FirstOrDefault(m=>m.To == Squares.C4),
                MoveProvider.GetMoves(Pieces.WhiteKnight,Squares.G1).FirstOrDefault(m=>m.To == Squares.F3)
            };
        }

        public virtual int Size => 0;

        public virtual IResult GetResult()
        {
            if (MoveHistory.GetPly() < 0)
            {
                return GetFirstMove();
            }
            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.GetResult();
            }
            return GetResult((short)-SearchValue, SearchValue, Depth);
        }

        protected IResult GetFirstMove()
        {
            return new Result
            {
                GameResult = GameResult.Continue,
                Move = _firstMoves[Random.Next() % _firstMoves.Length]
            };
        }

        public virtual IResult GetResult(short alpha, short beta, sbyte depth, MoveBase pv = null)
        {
            Result result = new Result();
            if (IsDraw(result))
            {
                return result;
            }

            SortContext sortContext = DataPoolService.GetCurrentSortContext();
            sortContext.Set(Sorters[Depth], pv);
            MoveList moves = Position.GetAllMoves(sortContext);

            DistanceFromRoot = sortContext.Ply; 
            MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

            if (CheckEndGame(moves.Count, result)) return result;

            if (moves.Count > 1)
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
        public virtual short Search(short alpha, short beta, sbyte depth)
        {
            if (depth < 1) return Evaluate(alpha, beta);

            if (Position.GetPhase() == Phase.End)
                return EndGameStrategy.Search(alpha, beta, (sbyte)Math.Min(depth + 1, MaxEndGameDepth));

            if (CheckDraw()) return 0;

            SearchContext context = GetCurrentContext(alpha, beta, depth);

            if(SetSearchValue(alpha, beta, depth, context))return context.Value;

            return context.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool SetSearchValue(short alpha, short beta, sbyte depth, SearchContext context)
        {
            switch (context.SearchResultType)
            {
                case SearchResultType.EndGame:
                    return true;
                case SearchResultType.AlphaFutility:
                    FutilitySearchInternal(alpha, beta, depth, context);
                    if (context.SearchResultType == SearchResultType.EndGame)
                    {
                        context.Value = alpha;
                        return true;
                    }
                    break;
                case SearchResultType.BetaFutility:
                    context.Value = beta;
                    return true;
                case SearchResultType.Razoring:
                    SearchInternal(alpha, beta, --depth, context);
                    break;
                case SearchResultType.None:
                    SearchInternal(alpha, beta, depth, context);
                    break;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void FutilitySearchInternal(short alpha, short beta, sbyte depth, SearchContext context)
        {
            MoveBase move;
            short r;
            sbyte d = (sbyte)(depth - 1);
            short b = (short)-beta;

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];

                Position.Make(move);

                if (!move.IsCheck && move.IsFutile)
                {
                    Position.UnMake();
                    continue;
                }

                r = (short)-Search(b, (short)-alpha, d);

                Position.UnMake();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move;

                if (r >= beta)
                {
                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
                else if (r > alpha)
                {
                    alpha = r;
                }
            }

            if (context.Value == short.MinValue)
            {
                context.SearchResultType  = SearchResultType.EndGame;
            }
            else
            {
                context.BestMove.History += 1 << depth;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void SearchInternal(short alpha, short beta, sbyte depth, SearchContext context)
        {
            if(MaxExtensionPly > context.Ply)
            {
                ExtensibleSearch(alpha, beta, depth, context);
            }
            else
            {
                RegularSearch(alpha, beta, depth, context);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void RegularSearch(short alpha, short beta, sbyte depth, SearchContext context) 
        {
            MoveBase move;
            short r;
            sbyte d = (sbyte)(depth - 1);
            short b = (short)-beta;

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];
                Position.Make(move);

                r = (short)-Search(b, (short)-alpha, d);

                Position.UnMake();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move;

                if (r >= beta)
                {
                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
                if (r > alpha)
                    alpha = r;
            }

            context.BestMove.History += 1 << depth;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ExtensibleSearch(short alpha, short beta, sbyte depth, SearchContext context)
        {
            if(context.Moves.Count < 2)
            {
                SingleMoveSearch(alpha, beta, depth, context);
            }
            else
            {
                ExtensibleSearchInternal(alpha, beta, depth, context);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void ExtensibleSearchInternal(short alpha, short beta, sbyte depth, SearchContext context)
        {
            MoveBase move;
            short r;
            sbyte d = (sbyte)(depth - 1);
            short b = (short)-beta;

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];
                Position.Make(move);

                sbyte extension = GetExtension(move);

                r = (short)-Search(b, (short)-alpha, (sbyte)(d + extension));

                Position.UnMake();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move;

                if (r >= beta)
                {
                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
                if (r > alpha)
                    alpha = r;
            }

            context.BestMove.History += 1 << depth;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SingleMoveSearch(short alpha, short beta, sbyte depth, SearchContext context)
        {
            Position.Make(context.Moves[0]);
            context.Value = (short)-Search((short)-beta, (short)-alpha, depth);
            context.BestMove = context.Moves[0];
            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void SetResult(short alpha, short beta, sbyte depth, Result result, MoveList moves)
        {
            short b = (short)-beta;
            sbyte d = (sbyte)(depth - 1);
            for (byte i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                short value = (short)-Search(b, (short)-alpha, d);

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
        protected virtual SearchContext GetCurrentContext(short alpha,short beta, sbyte depth, MoveBase pv = null)
        {
            SearchContext context = DataPoolService.GetCurrentContext();
            context.Clear();

            SortContext sortContext = DataPoolService.GetCurrentSortContext();
            sortContext.Set(Sorters[depth], pv);
            context.Moves = Position.GetAllMoves(sortContext);

            if (context.Moves.Count < 1)
            {
                context.SearchResultType = SearchResultType.EndGame;
                if (MoveHistory.IsLastMoveWasCheck())
                {
                    context.Value = (short)-EvaluationService.GetMateValue();
                }
                else
                {
                    context.Value = 0;
                }
            }
            else
            {
                context.SearchResultType = SetEndGameType(alpha, beta, depth);
            }

            return context;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MoveList SubSearch(MoveList moves, short alpha, short beta, sbyte depth)
        {
            if (UseSubSearch && Depth - depth < SubSearchLevel && depth - SubSearchDepth > SubSearchDepthThreshold)
            {
                ValueMove[] valueMoves = new ValueMove[moves.Count];
                for (byte i = 0; i < moves.Count; i++)
                {
                    Position.Make(moves[i]);

                    valueMoves[i] = new ValueMove { Move = moves[i], Value = -SubSearchStrategy.Search((short)-beta, (short)-alpha, (sbyte)(depth - SubSearchDepth)) };

                    Position.UnMake();
                }

                Array.Sort(valueMoves);

                moves.Clear();
                for (int i = 0; i < valueMoves.Length; i++)
                {
                    moves.Add(valueMoves[i].Move);
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
        protected SearchResultType SetEndGameType(short alpha, short beta, sbyte depth)
        {
            if (depth > RazoringDepth || MoveHistory.IsLastMoveWasCheck()) return SearchResultType.None;

            int value = Position.GetStaticValue();
            int margin = FutilityMargins[Position.GetPhase()][depth];

            if(depth < RazoringDepth)
            {
                if (value + margin < alpha) return SearchResultType.AlphaFutility;
                if (value - margin > beta) return SearchResultType.BetaFutility;
                return SearchResultType.None;
            }

            if(value + margin < alpha)
                return SearchResultType.Razoring;

            return SearchResultType.None;
        }

        protected virtual void InitializeSorters(short depth, IPosition position, MoveSorterBase mainSorter)
        {
            if (UseComplexSort)
            {
                int maxDepth = depth + 2;
                int complexDepth = Math.Max(maxDepth - (depth > 6 ? 3 : 2), 3);
                Sorters = new MoveSorterBase[maxDepth];

                var initialSorter = MoveSorterProvider.GetInitial(position, Sorting.Sort.HistoryComparer);
                var complexSorter = MoveSorterProvider.GetComplex(position, Sorting.Sort.HistoryComparer);
                Sorters[0] = MoveSorterProvider.GetAttack(position, Sorting.Sort.HistoryComparer);

                var d = SortDepth[depth] + 1;

                for (int i = 1; i < d; i++)
                {
                    Sorters[i] = mainSorter;
                }
                for (var i = d; i < complexDepth; i++)
                {
                    Sorters[i] = initialSorter;
                }
                for (var i = complexDepth; i < maxDepth; i++)
                {
                    Sorters[i] = complexSorter;
                }
            }
            else
            {
                int maxDepth = depth + 2;
                Sorters = new MoveSorterBase[maxDepth];

                var initialSorter = MoveSorterProvider.GetInitial(position, Sorting.Sort.HistoryComparer);
                Sorters[0] = MoveSorterProvider.GetAttack(position, Sorting.Sort.HistoryComparer);

                var d = SortDepth[depth] + 1;

                for (int i = 1; i < d; i++)
                {
                    Sorters[i] = mainSorter;
                }
                for (var i = d; i < maxDepth; i++)
                {
                    Sorters[i] = initialSorter;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected short Evaluate(short alpha, short beta)
        {
            if (MoveHistory.IsLastMoveWasCheck())
                return Search(alpha, beta, 1);

            short standPat = Position.GetValue();
            if (standPat >= beta)
                return beta;

            bool isDelta = false;

            if (standPat < alpha - DeltaMargins[Position.GetPhase()])
                isDelta = true;
            else if (alpha < standPat)
                alpha = standPat;

            SortContext sortContext = DataPoolService.GetCurrentSortContext();
            sortContext.SetForEvaluation(Sorters[0]);
            MoveList moves = Position.GetAllAttacks(sortContext);

            short b = (short)-beta;
            if (isDelta)
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    if (move.IsCheck || move.IsPromotionToQueen || move.IsQueenCaptured())
                    {
                        short score = (short)-Evaluate(b, (short)-alpha);

                        Position.UnMake();

                        if (score >= beta)
                            return beta;

                        if (score > alpha)
                            alpha = score;
                    }
                    else
                    {
                        Position.UnMake();
                    }
                }
            }
            else
            {
                for (byte i = 0; i < moves.Count; i++)
                {
                    Position.Make(moves[i]);

                    short score = (short)-Evaluate(b, (short)-alpha);

                    Position.UnMake();

                    if (score >= beta)
                        return beta;

                    if (score > alpha)
                        alpha = score;
                }
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
            if (count > 0) return false;

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

            if (MoveHistory.IsThreefoldRepetition(Position.GetKey())) return true;

            if (Position.GetPhase() == Phase.Middle) return false;

            return MoveHistory.IsFiftyMoves() || Position.IsDraw();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual sbyte GetExtension(MoveBase move)
        {
            return move.IsCheck || move.IsPromotionExtension ? One : Zero;
        }

        private void InitializeMargins()
        {
            FutilityMargins = new short[3][];
            FutilityMargins[0] = new short[]
            {
                EvaluationService.GetValue(0, Phase.Opening),
                EvaluationService.GetValue(2, Phase.Opening),
                (short)(EvaluationService.GetValue(3, Phase.Opening)+EvaluationService.GetValue(0, Phase.Opening)/2),
                EvaluationService.GetValue(4, Phase.Opening)
            };
            FutilityMargins[1] = new short[]
            {
                EvaluationService.GetValue(0, Phase.Middle),
                EvaluationService.GetValue(2, Phase.Middle),
                (short)(EvaluationService.GetValue(3, Phase.Middle)+EvaluationService.GetValue(0, Phase.Middle)/2),
                EvaluationService.GetValue(4, Phase.Middle)
            };
            FutilityMargins[2] = new short[]
            {
                EvaluationService.GetValue(0, Phase.End),
                EvaluationService.GetValue(2, Phase.End),
                (short)(EvaluationService.GetValue(3, Phase.End)+EvaluationService.GetValue(0, Phase.End)/2),
                EvaluationService.GetValue(4, Phase.End)
            };

            DeltaMargins = new short[3]
            {
                (short)(EvaluationService.GetValue(4, Phase.Opening)-EvaluationService.GetValue(0, Phase.Opening)),
                (short)(EvaluationService.GetValue(4, Phase.Middle)-EvaluationService.GetValue(0, Phase.Middle)),
                (short)(EvaluationService.GetValue(4, Phase.End)-EvaluationService.GetValue(0, Phase.End))
            };
        }
    }
}
