﻿namespace Engine.Models.Enums;

public static class  Phase
{
    public const byte Opening = 0;
    public const byte Middle = 1;
    public const byte End = 2;
}

public enum StrategyType
{
    NegaMax,
    LMR,
    LMRD,
    ASP,
    ID,
    NULL,
    Test
}