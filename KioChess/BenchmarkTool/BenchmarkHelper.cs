namespace EngineBenchmark
{
    public static class BenchmarkHelper
    {
        private  static string ErrorLogFile = "BenchmarkErrors.log";
        private static StreamWriter _errorLogWriter;
        public static Random Random { get; } = new Random();

        public static void InitializeLogger()
        {
            // Initialize logging or any other setup needed for benchmarks
            // This can include setting up log files, configuring log levels, etc.
            if (File.Exists(ErrorLogFile))
                File.Delete(ErrorLogFile);

            _errorLogWriter = new StreamWriter(ErrorLogFile, append: true);
        }

        public static void LogError(string message)
        {
            if (_errorLogWriter == null)
            {
                using (_errorLogWriter = new StreamWriter(ErrorLogFile, append: true))
                {
                    _errorLogWriter.WriteLine($"[ERROR] {DateTime.Now}: {message}");
                    _errorLogWriter.Flush();
                } 
            }
            else
            {
                _errorLogWriter.WriteLine($"[ERROR] {DateTime.Now}: {message}");
                _errorLogWriter.Flush();
            }
        }

        public static void CloseLogger()
        {
            _errorLogWriter?.Flush();
            // Ensure the log file is properly closed when done
            _errorLogWriter?.Close();
            _errorLogWriter = null;
        }
    }
}
