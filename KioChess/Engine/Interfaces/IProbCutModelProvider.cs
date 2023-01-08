using Engine.Strategies.Models;

namespace Engine.Interfaces
{
    public interface IProbCutModelProvider
    {
        ProbCutModel[] CreateModels(short depth);
    }
}
