using HtmlAgilityPack;
using log4net;
using Melon.Crawler.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Melon.Crawler
{
    public class Crawler
    {
        private static readonly int maxNumberOfThreads = 10;
        private static readonly ConcurrentDictionary<string, byte> urls = new ConcurrentDictionary<string, byte>();
        private static readonly ConcurrentDictionary<string, byte> blackList = new ConcurrentDictionary<string, byte>();
        private static readonly byte emptyPlaceHolder = 0;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(maxNumberOfThreads);
        private static Stopwatch stopwatch = new Stopwatch();
        private Extractor extractor = new Extractor();
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Reader reader = new Reader(Log);
        private UrlFormatter urlFormatter = new UrlFormatter(Log);



        public void GetUrls(string url)
        {
            reader.ParseRobotsTxt(url);

            if (reader.URLIsAllowed(url))
            {
                var initialPageHtml = reader.GetContentFromUrl(url);
                List<HtmlNode> nodesHtml = extractor.ExtractHrefs(initialPageHtml);
                ConcurrentBag<HtmlNode> hrefs = new ConcurrentBag<HtmlNode>(nodesHtml);

                stopwatch.Start();

                while (hrefs.Count > 0)
                {
                    List<HtmlNode> newHrefs = new List<HtmlNode>();
                    ConcurrentBag<HtmlNode> newNodes = new ConcurrentBag<HtmlNode>();
                    List<Task> tasks = new List<Task>();

                    foreach (var href in hrefs)
                    {
                        Task currentTask = Task.Run(() =>
                        {
                            semaphore.Wait();

                            var unformatedURL = href.ChildAttributes("href").FirstOrDefault().Value;

                            if (!blackList.ContainsKey(unformatedURL))
                            {
                                var formattedLink = urlFormatter.FormatLink(unformatedURL, url);

                                if (formattedLink == string.Empty)
                                {
                                    blackList.TryAdd(unformatedURL, emptyPlaceHolder);
                                }

                                if (formattedLink != string.Empty && !urls.ContainsKey(formattedLink))
                                {
                                    bool successfullyAdded = urls.TryAdd(formattedLink, emptyPlaceHolder);

                                    if (successfullyAdded)
                                    {
                                        if (reader.URLIsAllowed(formattedLink))
                                        {
                                            try
                                            {
                                                var currentPageContent = reader.GetContentFromUrl(formattedLink);

                                                if (currentPageContent != string.Empty)
                                                {
                                                    var currentPageHrefs = extractor.ExtractHrefs(currentPageContent);
                                                    newNodes.AddRange(currentPageHrefs);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error(ex.InnerException);
                                            }
                                        }
                                        else
                                        {
                                            Log.Warn($"Url: ({formattedLink}) is unavailable according to the site's robots.txt file");
                                        }

                                    }
                                    else
                                    {
                                        Log.Warn($"{formattedLink} was already added");
                                    }
                                }
                            }

                            semaphore.Release();

                        });

                        tasks.Add(currentTask);

                    }

                    Task.WaitAll(tasks.ToArray());
                    hrefs = newNodes;
                }
                stopwatch.Stop();
                Log.Info(stopwatch.Elapsed);
                Printer.PrintURLS(urls, url);
                semaphore.Dispose();
            }
            else
            {
                Log.Warn($"Url: ({url}) is unavailable according to the site's robots.txt file");
            }
        }
    }
}
