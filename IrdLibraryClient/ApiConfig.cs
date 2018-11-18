using NLog;

namespace IrdLibraryClient
{
    public static class ApiConfig
    {
        public static readonly ILogger Log;

        static ApiConfig()
        {
            Log = LogManager.GetLogger("default");
        }
    }
}