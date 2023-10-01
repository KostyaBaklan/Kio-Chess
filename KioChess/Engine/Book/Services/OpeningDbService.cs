using Engine.Book.Interfaces;
using Engine.Interfaces.Config;

namespace Engine.Book.Services
{
    public class OpeningDbService : DbServiceBase, IOpeningDbService
    {
        public OpeningDbService(IConfigurationProvider configuration) : base(configuration)
        {
        }

        public void AddOpening(IEnumerable<string> names)
        {
            throw new NotImplementedException();
        }

        public bool AddOpeningVariation(string name, short openingID, short variationID, List<string> moves)
        {
            throw new NotImplementedException();
        }

        public void AddVariations(IEnumerable<string> names)
        {
            throw new NotImplementedException();
        }

        public short GetOpeningID(string openingName)
        {
            throw new NotImplementedException();
        }

        public string GetOpeningName(string key)
        {
            throw new NotImplementedException();
        }

        public HashSet<string> GetOpeningNames()
        {
            throw new NotImplementedException();
        }

        public int GetOpeningVariationID(string key)
        {
            throw new NotImplementedException();
        }

        public HashSet<string> GetSequenceKeys()
        {
            throw new NotImplementedException();
        }

        public List<KeyValuePair<int, string>> GetSequences(string filter = null)
        {
            throw new NotImplementedException();
        }

        public HashSet<string> GetSequenceSets()
        {
            throw new NotImplementedException();
        }

        public short GetVariationID(string variationName)
        {
            throw new NotImplementedException();
        }

        public bool IsOpeningVariationExists(short openingID, short variationID)
        {
            throw new NotImplementedException();
        }

        public void SaveOpening(string key, int id)
        {
            throw new NotImplementedException();
        }
    }
}
