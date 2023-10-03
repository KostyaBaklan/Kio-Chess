

using System.ServiceModel;

namespace GamesServices;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISequenceService" in both code and config file together.
[ServiceContract]
public interface ISequenceService
{
    [OperationContract]
    void Initialize();

    [OperationContract]
    void CleanUp();

    [OperationContract]
    void ProcessSequence(string sequences);


}
