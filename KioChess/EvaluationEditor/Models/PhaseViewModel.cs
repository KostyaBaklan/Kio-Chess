using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Engine.Interfaces.Config;
using Engine.Models.Config;
using Engine.Models.Helpers;
using Prism.Mvvm;

namespace EvaluationEditor.Models;

public class PhaseViewModel:BindableBase
{
    public PhaseViewModel(IStaticValueProvider valueProvider, byte piece, byte phase)
    {
        //var moveProvider = ServiceLocator.Current.GetService<IMoveProvider>();
        Phase = phase;
        var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var labels = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };
        var array = numbers.Reverse().ToArray();
        Numbers = array;
        Labels = labels;

        Name = phase.ToString();
        Squares = new ObservableCollection<SquareViewModel>();
        for (int i = 0; i < 64; i++)
        {
            var x = i / 8;
            var y = i % 2;
            CellType cellType;

            if (x % 2 == 0)
            {
                cellType = y == 0 ? CellType.Black : CellType.White;
            }
            else
            {
                cellType = y == 1 ? CellType.Black : CellType.White;
            }

            var file = 7 - i / 8;
            var rank = i % 8;
            byte square = (byte)(file *8+rank);
            short value = (short) (valueProvider.GetValue(piece, phase, square));
            //if (piece % 6 == 0 )
            //{
            //    value *= 4;
            //}
            //else if (piece % 6 == 1|| piece % 6 == 2|| piece % 6 == 3|| piece % 6 == 5)
            //{
            //    value *= 2;
            //}
            //if(piece%6 == 0)
            //{
            //    if (file == 0 ||  file == 7)
            //    {
            //        value = 0;
            //    }
            //    else if (piece == 0)
            //    {
            //        if(file == 1)
            //        {
            //            value = 0;
            //        }
            //        else
            //        {
            //            value = (short)(file - 1);
            //        }
            //    }
            //    else
            //    {
            //        if (file == 6)
            //        {
            //            value = 0;
            //        }
            //        else
            //        {
            //            value = (short)(6-file);
            //        }
            //    }
            //}
            //else
            //{
            //    value = moveProvider.GetAttackPattern(piece, square).Count();
            //}
            Squares.Add(new SquareViewModel(square, value, cellType));
        }
    }

    public string Name { get; }

    public ObservableCollection<SquareViewModel> Squares { get; }

    private IEnumerable<int> _numbers;

    public IEnumerable<int> Numbers
    {
        get => _numbers;
        set => SetProperty(ref _numbers, value);
    }

    private IEnumerable<string> _labels;

    public IEnumerable<string> Labels
    {
        get => _labels;
        set => SetProperty(ref _labels, value);
    }

    public byte Phase { get; }

    public PhaseStaticTable ToTable()
    {
        var phaseStaticTable = new PhaseStaticTable(Phase);
        Dictionary<string, short> values = Squares.OrderBy(s => s.Square)
            .ToDictionary(k => k.Square.AsString(), v => v.Value);
        phaseStaticTable.Values = values;
        return phaseStaticTable;
    }
}