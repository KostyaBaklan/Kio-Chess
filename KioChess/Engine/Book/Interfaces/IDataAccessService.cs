﻿using Engine.Book.Models;
using Microsoft.Data.SqlClient;

namespace Engine.Book.Interfaces
{
    public interface IDataAccessService
    {
        void Upsert(string history, short key, GameValue value);
        void UpdateHistory(GameValue value);
        void Clear();
        void Connect();
        void Disconnect();
        void Execute(string sql);
        void Execute(string sql, string[] names, object[] values);
        IEnumerable<T> Execute<T>(string sql, Func<SqlDataReader, T> factory);
        bool Exists(string history, short key);
        void Export(string file);
        HistoryValue Get(string history);

        Task LoadAsync(IBookService bookService);
        void WaitToData();
        void SaveOpening(string key, int id);
        void SaveOpening(string opening, string variation, string sequence = null);
        HashSet<string> GetOpeningNames();
        string GetOpeningName(string key);
        void AddOpening(IEnumerable<string> names);
        void AddVariations(IEnumerable<string> names);
        short GetOpeningID(string openingName);
        short GetVariationID(string variationName);
        bool AddOpeningVariation(string name, short openingID, short variationID, List<string> moves);
        List<KeyValuePair<int, string>> GetSequences(string filter = null);
        bool IsOpeningVariationExists(short openingID, short variationID);
        List<HashSet<string>> GetSequences(int v);
        HashSet<string> GetSequenceKeys();
        int GetOpeningVariationID(string key);
        string GetMoves(string n1);
        HashSet<string> GetSequenceSets();
    }
}
