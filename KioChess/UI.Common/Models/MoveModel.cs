using Prism.Mvvm;

namespace UI.Common.Models
{
    public class MoveModel : BindableBase
    {
        private int _number;

        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        private string _white;

        public string White
        {
            get => _white;
            set => SetProperty(ref _white, value);
        }

        private string _black;

        public string Black
        {
            get => _black;
            set => SetProperty(ref _black, value);
        }

        private string _memory;

        public string Memory
        {
            get => _memory;
            set => SetProperty(ref _memory, value);
        }

        private string _time;

        public string Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private int _evaluation;

        public int Evaluation
        {
            get => _evaluation;
            set => SetProperty(ref _evaluation, value);
        }

        private double _table;

        public double Table
        {
            get => _table;
            set => SetProperty(ref _table, value);
        }

        private string _whiteValue;

        public string WhiteValue
        {
            get => _whiteValue;
            set => SetProperty(ref _whiteValue, value);
        }

        private string _blackValue;

        public string BlackValue
        {
            get => _blackValue;
            set => SetProperty(ref _blackValue, value);
        }
    }
}