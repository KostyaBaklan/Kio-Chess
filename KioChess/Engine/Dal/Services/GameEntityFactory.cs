using DataAccess.Entities;
using Engine.DataStructures;
using Engine.Models.Hash;

namespace Engine.Dal.Services;

/// <summary>
/// Factory for creating GameEntity records from move history with 128-bit hash support
/// Replaces Book record generation for new games.db pipeline
/// </summary>
public class GameEntityFactory
{
    private readonly int _depth;

    public GameEntityFactory(int depth)
    {
        _depth = depth;
    }

    /// <summary>
    /// Create GameEntity records from a game result
    /// Uses cumulative XOR for order-independent 128-bit hash computation
    /// </summary>
    public List<GameEntity> CreateRecords(MoveKeyList moveKeyList, int white, int draw, int black)
    {
        List<GameEntity> records = new(moveKeyList.Count);
        UInt128 cumulativeHash = UInt128.Zero;

        // Process all moves in a single loop starting from 0
        for (byte i = 0; i < moveKeyList.Count; i++)
        {
            short moveKey = moveKeyList[i];

            // Create record with current cumulative hash (before adding this move)
            records.Add(new GameEntity
            {
                Hash = cumulativeHash,
                NextMove = moveKey,
                White = white,
                Draw = draw,
                Black = black,
                Length = i
            });

            // XOR this move's hash for next iteration
            // XOR is commutative and associative - order doesn't matter!
            cumulativeHash ^= MoveHashSequenceHasher.GetMoveHash(moveKey);
        }

        return records;
    }
}
