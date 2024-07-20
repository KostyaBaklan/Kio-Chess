using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace Engine.Dal.Interfaces
{
    public interface ILocalDbService : IDbService
    {
        int GetPositionTotalDifferenceCount();
        void Add(IEnumerable<PositionTotalDifference> positions);
        void ClearPositionTotalDifference();

        IEnumerable<PositionTotalDifference> GetPositionTotalDifference();
        List<PositionTotalDifference> GetPositionTotalDifferenceList();
        string GetDebutName(byte[] key);
        List<Debut> GetAllDebuts();
        void AddDebuts(IEnumerable<Debut> debuts);
    }
}
