using Engine.Interfaces;
using Engine.Strategies.Models;

namespace Engine.Services;

public class ProbCutModelProvider : IProbCutModelProvider
{
    #region Implementation of IProbCutModelProvider

    public ProbCutModel[] CreateModels(short depth)
    {
        var models = new ProbCutModel[depth + 1];
        int depthLimit;
        if (depth%2 == 0)
        {
            depthLimit = Math.Max(4, depth / 2);
            for (int i = depthLimit; i <= depth; i++)
            {
                int d;
                if (i % 2 == 0)
                {
                    d = i / 2;
                    models[i] = new ProbCutModel(true, 1.5, 0.542, 1.036, -0.009, d);
                }
                else
                {
                    d = i / 2 + 1;
                    models[i] = new ProbCutModel(false, 1.5, 0.542, 1.036, -0.009, d);
                }

            } 
        }
        else
        {
            depthLimit = Math.Max(4, depth / 2);
            for (int i = depthLimit; i <= depth; i++)
            {
                int d;
                if (i % 2 == 1)
                {
                    d = i / 2;
                    models[i] = new ProbCutModel(true, 1.5, 0.542, 1.036, -0.009, d);
                }
                else
                {
                    d = i / 2 + 1;
                    models[i] = new ProbCutModel(false, 1.5, 0.542, 1.036, -0.009, d);
                }

            }
        }

        for (int i = 0; i < depthLimit; i++)
        {
            models[i] = new ProbCutModel(false, 1.5, 0.542, 1.036, -0.009, i);
        }

        return models;
    }

    #endregion
}