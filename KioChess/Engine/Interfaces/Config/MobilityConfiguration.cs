using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interfaces.Config
{
    public class MobilityConfiguration
    {
        public PhaseMobilityConfiguration[] Phases { get; set; }
    }

    public class PieceMobilityConfiguration
    {
        public byte Value { get; set; }
        public byte ZeroPenalty { get; set; }

    }

    public class PhaseMobilityConfiguration
    {
        public PieceMobilityConfiguration Knight { get; set; }
        public PieceMobilityConfiguration Bishop { get; set; }
        public PieceMobilityConfiguration Rook { get; set; }
    }
}
