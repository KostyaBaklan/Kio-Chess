using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class BookConfiguration : IBookConfiguration
    {
        public string Connection { get; set; }

        public short SuggestedThreshold { get; set; }

        public short NonSuggestedThreshold { get; set; }

        public short Depth { get; set; }
    }
}