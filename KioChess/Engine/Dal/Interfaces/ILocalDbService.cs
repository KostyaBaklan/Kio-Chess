using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace Engine.Dal.Interfaces
{
    public interface ILocalDbService : IDbService
    {
        int GetPositionTotalDifferenceCount();
        int GetPositionsCount();
        void Add(IEnumerable<PositionTotalDifference> positions);
        void Add(IEnumerable<PositionEntity> positions);
        void ClearPositionTotalDifference();

        IEnumerable<PositionTotalDifference> GetPositionTotalDifference();
        List<PositionTotalDifference> GetPositionTotalDifferenceList();
        List<PositionEntity> GetPositionTotalList();
        string GetDebutName(byte[] key);
        List<Debut> GetAllDebuts();
        void AddDebuts(IEnumerable<Debut> debuts);
        void Shrink();
    }
}
