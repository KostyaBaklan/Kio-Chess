using StockFishCore.Stockfish.Exceptions;
using StockFishCore.Stockfish.Models;

namespace StockFishCore.Stockfish
{
    public class Stockfish : IStockfish
    {
        #region private variables

        private const int MAX_TRIES = 10000;

        #endregion

        #region private properties

        private StockfishProcess _stockfish { get; set; }

        #endregion

        #region public properties

        public Settings Settings { get; set; }

        public int Depth { get; set; }

        #endregion

        #region constructor

        public Stockfish(string path, int depth = 2, int skills = 10)
        {
            Depth = depth;
            _stockfish = new StockfishProcess(path);
            _stockfish.Start();
            _stockfish.ReadLine();

            Settings = new Settings(skills);

            foreach (var property in Settings.GetPropertiesAsDictionary())
            {
                setOption(property.Key, property.Value);
            }

            startNewGame();
            send("position startpos");
        }

        #endregion

        #region private

        private void send(string command, int estimatedTime = 100)
        {
            _stockfish.WriteLine(command);
            Thread.Sleep(1);
        }

        private bool isReady()
        {
            string line = "empty";
            send("isready");
            var tries = 0;
            while (tries < MAX_TRIES)
            {
                ++tries;
                line = _stockfish.ReadLine();
                if (line == "readyok")
                {
                    return true;
                }
            }
            throw new MaxTriesException(tries, nameof(isReady), line);
        }

        private void setOption(string name, string value)
        {
            send($"setoption name {name} value {value}");
            if (!isReady())
            {
                throw new ApplicationException();
            }
        }

        private string movesToString(string[] moves) => string.Join(" ", moves);

        private void startNewGame()
        {
            send("ucinewgame");
            if (!isReady())
            {
                throw new ApplicationException();
            }
        }

        private void go() => send($"go depth {Depth}");

        private void goTime(int time) => send($"go movetime {time}", estimatedTime: time + 100);

        private List<string> readLineAsList()
        {
            var data = _stockfish.ReadLine();
            return data.Split(' ').ToList();
        }

        #endregion

        #region public

        public void SetPosition(string fen, params string[] moves)
        {
            send($"position fen {fen} moves {movesToString(moves)}");
        }

        public void SetPosition(params string[] moves)
        {
            send($"position startpos moves {movesToString(moves)}");
        }

        public string GetBoardVisual()
        {
            var line = "empty";
            send("d");
            var board = "";
            var lines = 0;
            var tries = 0;
            while (lines < 17)
            {
                if (tries > MAX_TRIES)
                {
                    throw new MaxTriesException(tries, nameof(GetBoardVisual), line);
                }

                var data = _stockfish.ReadLine();
                if (data.Contains("+") || data.Contains("|"))
                {
                    lines++;
                    board += $"{data}\n";
                }
                line = data;
                tries++;
            }

            return board;
        }

        public string GetFenPosition()
        {
            string line = "empty";
            send("d");
            var tries = 0;
            while (true)
            {
                if (tries > MAX_TRIES)
                {
                    throw new MaxTriesException(tries, nameof(GetFenPosition), line);
                }

                var data = readLineAsList();
                if (data[0] == "Fen:")
                {
                    return string.Join(" ", data.GetRange(1, data.Count - 1));
                }
                else
                {
                    line = string.Join(" ", data);
                }

                tries++;
            }
        }

        public void SetFenPosition(string fenPosition)
        {
            startNewGame();
            send($"position fen {fenPosition}");
        }

        public string GetBestMove()
        {
            var line = "empty";
            go();
            var tries = 0;
            while (true)
            {
                if (tries > MAX_TRIES)
                {
                    throw new MaxTriesException(tries, nameof(GetBestMove), line);
                }

                var data = readLineAsList();

                if (data[0] == "bestmove")
                {
                    if (data[1] == "(none)")
                    {
                        return null;
                    }

                    return data[1];
                }
                else
                {
                    line = string.Join(" ", data);
                }

                tries++;
            }
        }

        public string GetBestMoveTime(int time = 1000)
        {
            var line = "empty";
            goTime(time);
            var tries = 0;
            while (true)
            {
                if (tries > MAX_TRIES)
                {
                    throw new MaxTriesException(tries, nameof(GetBestMoveTime), line);
                }

                var data = readLineAsList();
                if (data[0] == "bestmove")
                {
                    if (data[1] == "(none)")
                    {
                        return null;
                    }

                    return data[1];
                }
                else
                {
                    line = string.Join(" ", data);
                }

                tries++;
            }
        }

        public bool IsMoveCorrect(string moveValue)
        {
            var line = "empty";
            send($"go depth 1 searchmoves {moveValue}");
            var tries = 0;
            while (true)
            {
                if (tries > MAX_TRIES)
                {
                    throw new MaxTriesException(tries, nameof(IsMoveCorrect), line);
                }

                var data = readLineAsList();
                if (data[0] == "bestmove")
                {
                    if (data[1] == "(none)")
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    line = string.Join(" ", data);
                }

                tries++;
            }
        }

        public Evaluation GetEvaluation()
        {
            var line = "empty";
            Evaluation evaluation = new Evaluation();
            var fen = GetFenPosition();
            Color compare;
            if (fen.Contains("w"))
            {
                compare = Color.White;
            }
            else
            {
                compare = Color.Black;
            }

            goTime(10000);
            var tries = 0;
            while (true)
            {
                if (tries > MAX_TRIES)
                {
                    throw new MaxTriesException(tries, nameof(GetEvaluation), line);
                }

                var data = readLineAsList();
                if (data[0] == "info")
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i] == "score")
                        {
                            int k;
                            if (compare == Color.White)
                            {
                                k = 1;
                            }
                            else
                            {
                                k = -1;
                            }

                            evaluation = new Evaluation(data[i + 1], Convert.ToInt32(data[i + 2]) * k);
                        }
                    }
                }

                if (data[0] == "bestmove")
                {
                    return evaluation;
                }
                else
                {
                    line = string.Join(" ", data);
                }

                tries++;
            }
        }

        #endregion
    }
}
