﻿using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Strategies.Models.Contexts;

namespace Engine.Interfaces;

public interface IDataPoolService
{
    SearchContext GetCurrentContext();
    SortContext GetCurrentEvaluationSortContext();
    bool[] GetCurrentLowSee();
    MoveList GetCurrentMoveList();
    SortContext GetCurrentSortContext();
    void Initialize(Position position);
}
