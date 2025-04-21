namespace PrismPlayer.Logging
{
    public static class LogHelper
    {
        private static readonly StreamWriter logFile = new("debug.log", true);

        public static void Log(string tag, object message)
        {
            logFile.WriteLine($"[{tag}] {message}");
            logFile.Flush();
        }
    }
}
