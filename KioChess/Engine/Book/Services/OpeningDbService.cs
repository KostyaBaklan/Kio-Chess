using Engine.Book.Helpers;
using Engine.Book.Interfaces;
using Engine.Interfaces.Config;
using Microsoft.Data.Sqlite;
using System.Text;

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

        public void FillData()
        {
            InsertOpenings();

            InsertVariations();

            InsertOpeningVariations();

            InsertOpeningSequences();
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

        private void InsertOpenings()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                try
                {
                    var insert = @"INSERT INTO Openings (Id, Name) VALUES ($I,$N)";

                    using (var command = Connection.CreateCommand(insert))
                    {
                        command.Parameters.Add(new SqliteParameter("$I", 0));
                        command.Parameters.Add(new SqliteParameter("$N", string.Empty));

                        var records = File.ReadLines(@"C:\Dev\ChessDB\SQL\Openings.csv")
                            .Select(l =>
                            {
                                var parts = l.Split(',', StringSplitOptions.None);

                                return new
                                {
                                    Id = int.Parse(parts[0]),
                                    Name = string.Join(", ", parts.Skip(1).Select(p => p.Trim('"')))
                                };
                            }).ToList();

                        foreach (var item in records)
                        {
                            command.Parameters[0].Value = item.Id;
                            command.Parameters[1].Value = item.Name;

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    transaction.Rollback();
                }
            }
        }

        private void InsertVariations()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                try
                {
                    var insert = @"INSERT INTO Variations (Id, Name) VALUES ($I,$N)";

                    using (var command = Connection.CreateCommand(insert))
                    {
                        command.Parameters.Add(new SqliteParameter("$I", 0));
                        command.Parameters.Add(new SqliteParameter("$N", string.Empty));

                        var records = File.ReadLines(@"C:\Dev\ChessDB\SQL\Variations.csv")
                            .Select(l =>
                            {
                                var parts = l.Split(',', StringSplitOptions.None);

                                return new
                                {
                                    Id = int.Parse(parts[0]),
                                    Name = string.Join(", ", parts.Skip(1).Select(p => p.Trim('"')))
                                };

                            }).ToList();

                        foreach (var item in records)
                        {
                            command.Parameters[0].Value = item.Id;
                            command.Parameters[1].Value = item.Name;

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    transaction.Rollback();
                }
            }
        }

        private void InsertOpeningVariations()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                try
                {
                    var insert = @"INSERT INTO OpeningVariations (Id, Name, OpeningID, VariationID, Moves) VALUES ($I,$N,$O,$V,$M)";

                    using (var command = Connection.CreateCommand(insert))
                    {
                        command.Parameters.Add(new SqliteParameter("$I", 0));
                        command.Parameters.Add(new SqliteParameter("$N", string.Empty));
                        command.Parameters.Add(new SqliteParameter("$O", 0));
                        command.Parameters.Add(new SqliteParameter("$V", 0));
                        command.Parameters.Add(new SqliteParameter("$M", string.Empty));

                        var records = File.ReadLines(@"C:\Dev\ChessDB\SQL\OpeningVariations.csv")
                            .Select(l =>
                            {
                                var parts = l.Split(',', StringSplitOptions.None);
                                var length = parts.Length;

                                var name = new StringBuilder(parts[1].Trim('"'));

                                for (int i = 2; i < length - 3; i++)
                                {
                                    name.Append($", {parts[i].Trim('"')}");
                                }

                                return new
                                {
                                    Id = int.Parse(parts[0]),
                                    Name = name.ToString(),
                                    OpeningID = int.Parse(parts[length - 3]),
                                    VariationID = int.Parse(parts[length - 2]),
                                    Moves = parts[length - 1].Trim('"')
                                };
                            }).ToList();

                        foreach (var item in records)
                        {
                            command.Parameters[0].Value = item.Id;
                            command.Parameters[1].Value = item.Name;
                            command.Parameters[2].Value = item.OpeningID;
                            command.Parameters[3].Value = item.VariationID;
                            command.Parameters[4].Value = item.Moves;

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    transaction.Rollback();
                }
            }
        }

        private void InsertOpeningSequences()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                try
                {
                    var insert = @"INSERT INTO OpeningSequences (Id, Sequence, OpeningVariationID) VALUES ($I,$N,$O)";

                    using (var command = Connection.CreateCommand(insert))
                    {
                        command.Parameters.Add(new SqliteParameter("$I", 0));
                        command.Parameters.Add(new SqliteParameter("$N", string.Empty));
                        command.Parameters.Add(new SqliteParameter("$O", 0));

                        var records = File.ReadLines(@"C:\Dev\ChessDB\SQL\OpeningSequences.csv")
                            .Select(l =>
                            {
                                var parts = l.Split(',', StringSplitOptions.None);

                                return new
                                {
                                    Id = int.Parse(parts[0]),
                                    Sequence = parts[1].Trim('"'),
                                    OpeningVariationID = int.Parse(parts[2])
                                };
                            }).ToList();

                        foreach (var item in records)
                        {
                            command.Parameters[0].Value = item.Id;
                            command.Parameters[1].Value = item.Sequence;
                            command.Parameters[2].Value = item.OpeningVariationID;

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    transaction.Rollback();
                }
            }
        }
    }
}
