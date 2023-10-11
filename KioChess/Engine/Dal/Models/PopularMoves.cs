﻿using DataAccess.Models;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Dal.Models;

public interface IPopularMoves
{
    bool IsEmpty { get; }

    bool IsPopular(MoveBase move);
}
public class PopularMoves0 : IPopularMoves
{
    public PopularMoves0(params BookMove[] moves)
    {

    }

    public bool IsEmpty => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPopular(MoveBase move)
    {
        return false;
    }
}
public class PopularMoves1 : IPopularMoves
{
    private BookMove _move1;
    public PopularMoves1(params BookMove[] moves)
    {
        _move1 = moves[0];
    }

    public bool IsEmpty => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPopular(MoveBase move)
    {
        return _move1.Id == move.Key;
    }
}
public class PopularMoves2 : IPopularMoves
{
    private BookMove _move1;
    private BookMove _move2;
    public PopularMoves2(params BookMove[] moves)
    {
        _move1 = moves[0];
        _move2 = moves[1];
    }

    public bool IsEmpty => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPopular(MoveBase move)
    {
        if (_move1.Id == move.Key)
        {
            move.BookValue = _move1.Value;
            return true;
        }
        if (_move2.Id == move.Key)
        {
            move.BookValue = _move2.Value;
            return true;
        }
        return false;
    }
}
public class PopularMoves3 : IPopularMoves
{
    private BookMove _move1;
    private BookMove _move2;
    private BookMove _move3;
    public PopularMoves3(params BookMove[] moves)
    {
        _move1 = moves[0];
        _move2 = moves[1];
        _move3 = moves[2];
    }

    public bool IsEmpty => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPopular(MoveBase move)
    {
        if (_move1.Id == move.Key)
        {
            move.BookValue = _move1.Value;
            return true;
        }
        if (_move2.Id == move.Key)
        {
            move.BookValue = _move2.Value;
            return true;
        }
        if (_move3.Id == move.Key)
        {
            move.BookValue = _move3.Value;
            return true;
        }
        return false;
    }
}
public class PopularMoves4 : IPopularMoves
{
    private BookMove[] _move;
    public PopularMoves4(params BookMove[] moves)
    {
        _move = moves;
    }

    public bool IsEmpty => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPopular(MoveBase move)
    {
        for (int i = 0; i < _move.Length; i++)
        {
            if (_move[i].Id != move.Key)
                continue;

            move.BookValue = _move[i].Value;
            return true;
        }

        return false;
    }
}