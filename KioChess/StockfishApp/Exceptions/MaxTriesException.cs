﻿namespace StockfishApp.Exceptions
{
    public class MaxTriesException : Exception
    {
        public MaxTriesException(string msg = "") : base(msg) { }
    }
}