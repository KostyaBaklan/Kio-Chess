using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Services;

namespace Engine.DataStructures.Moves.Collections;

public abstract class MoveCollectionBase //: IMoveCollection
{
    protected MoveBase[] Moves;

    protected MoveCollectionBase()
    {
        Moves = ServiceLocator.Current.GetInstance<MoveProvider>()
            .GetAll()
            .ToArray();
    }

    public abstract MoveList Build();

    public abstract MoveList BuildBook();
}