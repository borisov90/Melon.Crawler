using log4net;
using System;
using System.Reflection;

namespace Melon.Crawler.Helpers
{
    public class UrlFormatter
    {
        private static ILog Log;

        public UrlFormatter(ILog logger)
        {
            Log = logger;
        }

        public string FormatLink(string link, string startingUrl)
        {
            var formattedUrl = string.Empty;

            try
            {
                Uri url = new Uri(startingUrl);
                var domainUrl = url.Scheme + Uri.SchemeDelimiter + url.Host;

                if (link == "/")
                {
                    formattedUrl = domainUrl;
                }
                else if (link.StartsWith("/"))
                {
                    formattedUrl = domainUrl + link;
                }
                else
                {
                    try
                    {
                        var currentLink = new Uri(link);

                        var currentDomain = currentLink.Host;

                        if (currentDomain == url.Host)
                        {
                            formattedUrl = currentLink.AbsoluteUri;
                        }
                    }
                    catch (Exception)
                    {
                        Log.Error($"Error: not a valid link: {link}");
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error while formatting: {ex.InnerException}");
            }
            

            return formattedUrl;
        }

        internal bool IsValidUrlFormat(string initialUrl)
        {
            bool validUrl = Uri.IsWellFormedUriString(initialUrl, UriKind.Absolute);

            return validUrl;
        }

        public string FormatLink(string link)
        {
            var formattedDomain = string.Empty;

            bool validUrl = IsValidUrlFormat(link);

            if (validUrl)
            {
                formattedDomain = new Uri(link).AbsoluteUri;
            }
            else
            {
                Log.Error($"Error: not a valid link: {link}");
            }

            return formattedDomain;
        }
    }
}
