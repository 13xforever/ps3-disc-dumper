using System;
using System.Net.Http;

namespace IrdLibraryClient.Utils
{
    public static class ConsoleLogger
    {
        public static void PrintError(Exception e, HttpResponseMessage response, bool isError = true)
        {
            if (isError)
                ApiConfig.Log.Error(e, "HTTP error");
            else
                ApiConfig.Log.Warn(e, "HTTP error");
            if (response == null)
                return;

            try
            {
                ApiConfig.Log.Info(response.RequestMessage.RequestUri);
                var msg = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                ApiConfig.Log.Warn(msg);
            }
            catch { }
        }
    }
}
