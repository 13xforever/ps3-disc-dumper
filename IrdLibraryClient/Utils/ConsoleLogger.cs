using System;
using System.Net.Http;

namespace IrdLibraryClient.Utils
{
    public static class ConsoleLogger
    {
        public static void PrintError(Exception e, HttpResponseMessage response, bool isError = true)
        {
            if (isError)
                Log.Error(e, "HTTP error");
            else
                Log.Warn(e, "HTTP error");
            if (response == null)
                return;

            try
            {
                Log.Info(response.RequestMessage.RequestUri.ToString());
                var msg = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Log.Warn(msg);
            }
            catch { }
        }
    }
}
