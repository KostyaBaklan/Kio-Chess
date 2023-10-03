using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class SmallCastle : MoveBase
{
    public SmallCastle()
    {
        IsCastle = true;
    }
}

public class WhiteSmallCastle : SmallCastle
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal()
    {
        return Board.CanDoWhiteSmallCastle();
    }

    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.DoWhiteSmallCastle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.UndoWhiteSmallCastle();
    }

    #endregion
}
public class BlackSmallCastle : SmallCastle
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal()
    {
        return Board.CanDoBlackSmallCastle();
    }

    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.DoBlackSmallCastle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.UndoBlackSmallCastle();
    }

    #endregion
}