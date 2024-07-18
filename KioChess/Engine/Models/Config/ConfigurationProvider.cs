using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class ConfigurationProvider: IConfigurationProvider
{
    public ConfigurationProvider(IAlgorithmConfiguration algorithmConfiguration, IEvaluationProvider evaluation,
        IGeneralConfiguration generalConfiguration, 
        IEndGameConfiguration endGameConfiguration, IBookConfiguration bookConfiguration)
    {
        AlgorithmConfiguration = algorithmConfiguration;
        Evaluation = evaluation;
        GeneralConfiguration = generalConfiguration;
        EndGameConfiguration = endGameConfiguration;
        BookConfiguration = bookConfiguration;
    }

    #region Implementation of IConfigurationProvider

    public IGeneralConfiguration GeneralConfiguration { get;  }
    public IAlgorithmConfiguration AlgorithmConfiguration { get; }
    public IEvaluationProvider Evaluation { get; }

    public IEndGameConfiguration EndGameConfiguration { get; }

    public IBookConfiguration BookConfiguration { get; }

    #endregion
}