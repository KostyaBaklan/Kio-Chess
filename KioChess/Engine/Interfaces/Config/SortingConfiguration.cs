﻿namespace Engine.Interfaces.Config
{
    public class SortingConfiguration
    {
        public int[] SortDepth { get; set; }
        public SortType SortType { get; set; }
        public int SortMinimum { get; set; }
        public int SortHalfIndex { get; set; }
        public int SortMoveIndex { get; set; }
        public bool UseComplexSort { get; set; }
    }
}