﻿using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class BigCastle : MoveBase
{
    public BigCastle()
    {
        IsCastle = true;
    }
}
public class WhiteBigCastle : BigCastle
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.CanDoWhiteBigCastle();

    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make() => Board.DoWhiteBigCastle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake() => Board.UndoWhiteBigCastle();

    #endregion
}
public class BlackBigCastle : BigCastle
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.CanDoBlackBigCastle();

    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make() => Board.DoBlackBigCastle();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake() => Board.UndoBlackBigCastle();

    #endregion
}