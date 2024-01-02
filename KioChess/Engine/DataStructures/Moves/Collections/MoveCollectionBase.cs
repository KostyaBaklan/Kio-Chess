using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Collections;

public abstract class MoveCollectionBase //: IMoveCollection
{
    protected MoveBase[] Moves;

    protected MoveCollectionBase()
    {
        Moves = ServiceLocator.Current.GetInstance<IMoveProvider>()
            .GetAll()
            .ToArray();
    }

    public abstract MoveList Build();

    public abstract MoveList BuildBook();
}