using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melon.Crawler.Helpers;
using log4net;
using System.Reflection;

namespace Melon.Crawler
{
    public class Program
    {
        public static Crawler crawler = new Crawler();
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static UrlFormatter urlFormatter = new UrlFormatter(Log);

        public static void Main(string[] args)
        {
            Log.Info("Enter starting url:");
            var initialUrl = Console.ReadLine();
            bool isValidUrl = urlFormatter.IsValidUrlFormat(initialUrl);

            while (!isValidUrl)
            {
                Log.Warn("This is not a valid web adress. Try again.");

                initialUrl = Console.ReadLine();
                isValidUrl = urlFormatter.IsValidUrlFormat(initialUrl);
            }
           
            var formattedInitialUrl = urlFormatter.FormatLink(initialUrl);
            Task task = Task.Factory.StartNew(() => crawler.GetUrls(formattedInitialUrl));
            Console.ReadLine();
        }
    }
}
