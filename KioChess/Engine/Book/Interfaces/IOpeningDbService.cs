namespace Engine.Book.Interfaces
{
    public interface IOpeningDbService : IDbService
    {
        void SaveOpening(string key, int id);
        HashSet<string> GetOpeningNames();
        string GetOpeningName(string key);
        void AddOpening(IEnumerable<string> names);
        void AddVariations(IEnumerable<string> names);
        short GetOpeningID(string openingName);
        short GetVariationID(string variationName);
        bool AddOpeningVariation(string name, short openingID, short variationID, List<string> moves);
        List<KeyValuePair<int, string>> GetSequences(string filter = null);
        bool IsOpeningVariationExists(short openingID, short variationID);
        HashSet<string> GetSequenceKeys();
        int GetOpeningVariationID(string key);
        HashSet<string> GetSequenceSets();
        void FillData();
    }
}
