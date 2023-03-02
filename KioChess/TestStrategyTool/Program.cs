using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Newtonsoft.Json;
using TestStrategyTool;
using Tools.Common;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();

        //Debugger.Launch();

        IPosition position = new Position();

        var depth = short.Parse(args[0]); 
        
        var moves = GameProvider.GetMoves(args[1]);

        position.MakeFirst(moves[0]);
        for (int i = 1; i < moves.Count; i++)
        {
            MoveBase moveBase = moves[i];
            position.Make(moveBase);
        }

        TestStrategy strategy = new TestStrategy(depth, position);

        var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        string input = Console.ReadLine();

        while(input != "end")
        {
            while (strategy.IsBlocked())
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
            }

            var moveID = short.Parse(input);

            var move = moveProvider.Get(moveID);

            position.Make(move);

            var result = strategy.GetResult();

            if (result.GameResult == Engine.DataStructures.GameResult.Continue)
            {
                position.Make(result.Move);
            }

            strategy.ExecuteAsyncAction();

            var dto = result.Todto();

            var json = JsonConvert.SerializeObject(dto);

            Console.WriteLine(json);

            input = Console.ReadLine();
        }
    }
}