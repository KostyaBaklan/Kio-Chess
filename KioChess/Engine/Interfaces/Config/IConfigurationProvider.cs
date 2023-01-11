﻿using Engine.Models.Config;

namespace Engine.Interfaces.Config
{
    public interface IConfigurationProvider
    {
        IGeneralConfiguration GeneralConfiguration { get; }
        IEndGameConfiguration EndGameConfiguration { get; }
        IAlgorithmConfiguration AlgorithmConfiguration { get; }
        IEvaluationProvider Evaluation { get; }
        IPieceOrderConfiguration PieceOrderConfiguration { get; }
    }
}
