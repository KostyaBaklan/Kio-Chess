﻿using Prism.Mvvm;

namespace DataViewer.Models
{
    public class DataModel : BindableBase
    {
        private int _number;

        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        private int _whiteCount;

        public int WhiteCount
        {
            get => _whiteCount;
            set => SetProperty(ref _whiteCount, value);
        }

        private int _drawCount;

        public int DrawCount
        {
            get => _drawCount;
            set => SetProperty(ref _drawCount, value);
        }

        private int _blackCount;

        public int BlackCount
        {
            get => _blackCount;
            set => SetProperty(ref _blackCount, value);
        }

        private double _whitePercentage;

        public double WhitePercentage
        {
            get => _whitePercentage;
            set => SetProperty(ref _whitePercentage, value);
        }

        private double _drawPercentage;

        public double DrawPercentage
        {
            get => _drawPercentage;
            set => SetProperty(ref _drawPercentage, value);
        }

        private double _blackPercentage;

        public double BlackPercentage
        {
            get => _blackPercentage;
            set => SetProperty(ref _blackPercentage, value);
        }
    }
}