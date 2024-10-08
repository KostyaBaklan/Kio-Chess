﻿namespace Tools.Common;

public static class RandomExtensions
{
    public static Random Random = new Random();

    public static T GetRandomItem<T>(this List<T> list) => list[Random.Next(list.Count)];
}
