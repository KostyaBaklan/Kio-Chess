using Engine.Interfaces.Config;

namespace Engine.Strategies.Lmr
{
    public class LmrTables
    {
        private readonly int Depth;

        private readonly int MaxMoveCount;

        private int ReducableDepth;
        private readonly int NonLmrOffset;
        private readonly int LmrOffset;
        private readonly int Ratio;
        private readonly int DeepRatio;

        public readonly bool[] CanReduceDepth;

        public readonly bool[][][] CanReduceMoveMax;

        public readonly sbyte[][][] ReductionMax;

        public LmrTables(IConfigurationProvider configurationProvider, int depth, int[] lmrConfig, int[] lmrRatio)
        {
            Depth = depth + 4;
            MaxMoveCount = configurationProvider.GeneralConfiguration.MaxMoveCount; 

            ReducableDepth = lmrConfig[0];
            NonLmrOffset = lmrConfig[1];
            LmrOffset = lmrConfig[2];

            Ratio = lmrRatio[0];
            DeepRatio = lmrRatio[1];

            CanReduceDepth = InitializeReducableDepthTable();
            ReductionMax = InitializeReductionMaxTable();
            CanReduceMoveMax = InitializeReducableMaxMoveTable();
        }

        private bool[] InitializeReducableDepthTable()
        {
            var result = new bool[Depth];
            for (int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = depth > ReducableDepth;
            }

            return result;
        }

        private bool[][][] InitializeReducableMaxMoveTable()
        {
            var result = new bool[Depth][][];
            for (int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = new bool[MaxMoveCount][];
                for (int move = 0; move < result[depth].Length; move++)
                {
                    result[depth][move] = new bool[move];
                    for (int i = 0; i < result[depth][move].Length; i++)
                    {
                        result[depth][move][i] = depth - ReductionMax[depth][move][i] > 1;
                    }
                }
            }
            return result;
        }
        private sbyte[][][] InitializeReductionMaxTable()
        {
            var result = new sbyte[Depth][][];
            for (int depth = 0; depth < result.Length; depth++)
            {
                result[depth] = new sbyte[MaxMoveCount][];
                for (int move = 0; move < result[depth].Length; move++)
                {
                    result[depth][move] = new sbyte[move];
                    for (int i = 0; i < result[depth][move].Length; i++)
                    {
                        if (depth > ReducableDepth + 1)
                        {
                            result[depth][move][i] = GetOnReducableDepth(depth, move, i);
                        }
                        else if (depth > ReducableDepth)
                        {
                            result[depth][move][i] = GetReducableDepth(depth, move, i);
                        }
                        else
                        {
                            result[depth][move][i] = (sbyte)(depth - 1);
                        }
                    }
                }
            }

            return result;
        }

        private sbyte GetOnReducableDepth(int depth, int move, int i) => i > LmrOffset + GetDeepOffset(depth, move) ? (sbyte)(depth - 3) : GetReducableDepth(depth, move, i);

        private sbyte GetReducableDepth(int depth, int move, int i) => i > NonLmrOffset + GetOffset(depth, move) ? (sbyte)(depth - 2) : (sbyte)(depth - 1);

        private int GetOffset(int depth, int move)
        {
            return move / Ratio;
        }

        private int GetDeepOffset(int depth, int move)
        {
            return move / DeepRatio;
        }
    }
}
