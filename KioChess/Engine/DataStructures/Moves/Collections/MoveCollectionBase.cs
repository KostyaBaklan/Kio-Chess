using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections;

public abstract class MoveCollectionBase //: IMoveCollection
{
    protected MoveBase[] Moves;
    protected readonly IMoveComparer Comparer;

    protected MoveCollectionBase(IMoveComparer comparer)
    {
        Comparer = comparer;
        Moves = ServiceLocator.Current.GetInstance<IMoveProvider>()
            .GetAll()
            .ToArray();
    }

    public abstract MoveList Build();

    public abstract MoveList BuildBook();
}