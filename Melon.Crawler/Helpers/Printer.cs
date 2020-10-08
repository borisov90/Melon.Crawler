using log4net;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Melon.Crawler.Helpers
{
    public static class Printer
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void PrintURLS(ConcurrentDictionary<string, byte> urls, string homeURL)
        {
            StringBuilder urlsToPrint = new StringBuilder("List of Url's in " + homeURL).Append(Environment.NewLine);

            foreach (var url in urls)
            {
                urlsToPrint.AppendLine(url.Key);
            }

            Log.Info(urlsToPrint);

            Log.Info($"Count { urls.Count() }");
        }
    }
}
