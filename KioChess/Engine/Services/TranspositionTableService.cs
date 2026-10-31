using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;

namespace Engine.Services;

public class TranspositionTableService : ITranspositionTableService
{
    private readonly IConfigurationProvider _configurationProvider;

    public TranspositionTableService(IConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }
    public TranspositionTable Create(int depth, Board board)
    {
        var capacity = _configurationProvider.GeneralConfiguration.TranspositionTable
            .MainTableSize[depth];
        return new TranspositionTable(capacity, board, _configurationProvider);
    }

    public TranspositionTable CreateNull(int depth, Board board)
    {
        var capacity = _configurationProvider.GeneralConfiguration.TranspositionTable
            .NullTableSize[depth];
        return new TranspositionTable(capacity, board, _configurationProvider);
    }
}
