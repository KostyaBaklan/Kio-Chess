using StockFishCore.Data;

namespace StockFishCore.Execution
{
    public static class BranchFactory
    {
        public static BranchItem Create()
        {
            Console.WriteLine("Please enter Branch Name:");
            string branch = Console.ReadLine();
            Console.WriteLine("Please enter Branch Description:");
            string description = Console.ReadLine();

            return Create(branch, description);
        }

        public static BranchItem Create(string branch, string description)
        {
            BranchItem item = new BranchItem()
            {
                Name = branch,
                Description = description
            };

            using (var db = new ResultContext())
            {
                if (db.RunTimeInformation.Any(x => x.Branch == branch))
                {
                    return null;
                }

                RunTimeInformation rti = new RunTimeInformation
                {
                    Branch = branch,
                    Description = description,
                    RunTime = DateTime.Now
                };

                db.RunTimeInformation.Add(rti);
                db.SaveChanges();

                item.Id = rti.Id;
            }

            return item;
        }
    }
}
