using Engine.Models.Config;

namespace Engine.Interfaces.Config;

public interface IConfigurationProvider
{
    IGeneralConfiguration GeneralConfiguration { get; }
    IBookConfiguration BookConfiguration { get; }
    IEndGameConfiguration EndGameConfiguration { get; }
    IAlgorithmConfiguration AlgorithmConfiguration { get; }
    IEvaluationProvider Evaluation { get; }
    IPieceOrderConfiguration PieceOrderConfiguration { get; }
}
