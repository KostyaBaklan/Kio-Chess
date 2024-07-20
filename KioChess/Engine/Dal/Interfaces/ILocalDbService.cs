using DataAccess.Interfaces;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Dal.Interfaces
{
    public interface ILocalDbService : IDbService
    {
        void Add(IEnumerable<PositionTotalDifference> positions);
        void ClearPositionTotalDifference();

        IEnumerable<PositionTotalDifference> GetPositionTotalDifference();
        List<PositionTotalDifference> GetPositionTotalDifferenceList();
    }
}
