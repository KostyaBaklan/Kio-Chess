using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class BookConfiguration : IBookConfiguration
    {
        public Dictionary<string,string> Connection { get; set; }

        public short SuggestedThreshold { get; set; }

        public short NonSuggestedThreshold { get; set; }

        public short Depth { get; set; }
    }
}