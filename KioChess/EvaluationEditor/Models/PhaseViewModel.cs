﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Engine.Interfaces.Config;
using Engine.Models.Config;
using Engine.Models.Helpers;
using Engine.Services;
using Prism.Mvvm;
using Prism.Ioc;

namespace EvaluationEditor.Models;

public class PhaseViewModel:BindableBase
{
    public PhaseViewModel(IStaticValueProvider valueProvider, byte piece, byte phase)
    {
        var moveProvider = ContainerLocator.Current.Resolve<MoveProvider>();
        Phase = phase;
        var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var labels = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };
        var array = numbers.Reverse().ToArray();
        Numbers = array;
        Labels = labels;

        Name = phase.ToString();
        Squares = new ObservableCollection<SquareViewModel>();

        int[] minValueTable = new int[12];
        for (byte i = 0; i < 12; i++)
        {
            int min = short.MaxValue;
            for (byte j = 0; j < 64; j++)
            {
                var value = moveProvider.GetAttackPattern(i, j).Count();
                if(value < min)
                {
                    min = value;
                }
            }

            minValueTable[i] = min;
        }

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
            short value = (short) valueProvider.GetValue(piece, phase, square);
            //if(piece %6!=0 && piece % 6 != 5)
            //{
            //    value = (short)(5*(moveProvider.GetAttackPattern(piece, square).Count() - minValueTable[piece]));
            //}
            //else if(piece%6 == 5)
            //{
            //    value *= 2;
            //}
            //if(piece%6 == 1)
            //{
            //    value /= 3;
            //}
            //else if(piece % 6 == 5)
            //{
            //    if (phase < 2)
            //    {
            //        value = 0;
            //    }
            //    else
            //    {
            //        value = moveProvider.GetAttackPattern(piece, square).Count();
            //    }
            //}
            //else if (piece % 6 == 4)
            //{

            //}
            //else
            //{
            //    value /= 2;
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