using log4net;
using System;
using System.Collections;
using System.Net;
using System.Reflection;

namespace Melon.Crawler.Helpers
{
    public class Reader
    {
        private static readonly string _robotAgent = "Mozilla 5.0; Melon Crawler - Georgi Borisov / Senior.NET Developer";
        private static string _content = string.Empty;
        private static int _statusCode = -1;
        private static string _lastError = string.Empty;
        private static ArrayList _BlockedUrls = new ArrayList();
        private static ILog Log;

        public Reader(ILog logger)
        {
            Log = logger;
        }

        public string FindRobotsTxt(string url)
        {
            Uri currentUrl = new Uri(url);
            string RobotsTxtFile = "http://" + currentUrl.Authority + "/robots.txt";
            string fileContent = GetContentFromUrl(RobotsTxtFile);

            Log.Info($"{RobotsTxtFile} was read");

            return fileContent;
        }

        public void ParseRobotsTxt(string URL)
        {
            string fileContent = FindRobotsTxt(URL);

            if (!String.IsNullOrEmpty(fileContent))
            {
                string[] fileLines = fileContent.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                bool ApplyToBot = false;
                foreach (string line in fileLines)
                {
                    Log.Info("Line = " + line);
                    RobotCommand CommandLine = new RobotCommand(line);
                    Log.Info("Command = " + CommandLine.Command + " - URL = " + CommandLine.Url + " - UserAgent: " + CommandLine.UserAgent);

                    switch (CommandLine.Command)
                    {
                        case "COMMENT":   // ingoring comments
                            break;
                        case "user-agent": 
                            if ((CommandLine.UserAgent.IndexOf("*") >= 0) || (CommandLine.UserAgent.IndexOf(_robotAgent) >= 0))
                            {
                                // if DISALLOW is on next line we will add the URL to our array of URls we cannot access
                                ApplyToBot = true;
                                Log.Info(CommandLine.UserAgent + " - Rules apply");
                            }
                            else
                            {
                                ApplyToBot = false;
                            }
                            break;
                        case "disallow": 
                            if (ApplyToBot)
                            {
                                if (CommandLine.Url.Length > 0)
                                {
                                    _BlockedUrls.Add(CommandLine.Url.ToLower());
                                    Log.Warn("DISALLOW " + CommandLine.Url);
                                }
                                else
                                {
                                    Log.Info("ALLOW ALL URLS - BLANK");
                                }
                            }
                            else
                            {
                                Log.Info("DISALLOW " + CommandLine.Url + " for another user-agent");
                            }
                            break;
                        case "allow":
                            Log.Info("ALLOW: " + CommandLine.Url);
                            break;
                        case "sitemap":
                            Log.Info("SITEMAP: " + CommandLine.Url);
                            break;
                        default:
                            // empty/unknown/error
                            Log.Info("# Unrecognised robots.txt entry [" + line + "]");
                            break;
                    }
                }
            }
        }

        public string GetContentFromUrl(string URL)
        {
            Log.Info("Accessing URL: " + URL);

            _content = string.Empty;
            _lastError = ""; 
            _statusCode = -1;

            WebClient client = new WebClient();

            client.Headers["User-Agent"] = _robotAgent; 
            client.Encoding = System.Text.Encoding.UTF8;

            try
            {
                _content = client.DownloadString(URL);
                _statusCode = 200; 
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    _lastError = "Bad Domain Name";
                }
                else if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse HTTPResponse = (HttpWebResponse)ex.Response;

                    _statusCode = (int)HTTPResponse.StatusCode;

                    if (HTTPResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        _lastError = "Page Not Found";
                    }
                    else if (HTTPResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        _lastError = "Access Forbidden";
                    }
                    else if (HTTPResponse.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        _lastError = "Server Error";
                    }
                    else if (HTTPResponse.StatusCode == HttpStatusCode.Gone)
                    {
                        _lastError = "Page No Longer Available";
                    }
                    else
                    {
                        _lastError = HTTPResponse.StatusDescription;
                    }
                }
                else
                {
                    _lastError = "Error: " + ex.ToString();
                }
            }
            catch (Exception ex)
            {
                _lastError = "Error: " + ex.ToString();
            }
            finally
            {
                client.Dispose();
            }

            if (!String.IsNullOrEmpty(_lastError))
            {
                Log.Error(_statusCode.ToString() + ": " + _lastError);
            }

            return _content;
        }

        public bool URLIsAllowed(string URL)
        {
            if (_BlockedUrls.Count == 0) return true;

            Uri checkURL = new Uri(URL);
            URL = checkURL.AbsolutePath.ToLower();

            Log.Info("Is user-agent: " + _robotAgent + " allowed access to URL: " + URL);

            // if URL is the /robots.txt then don't allow it as we should use the ParseRobotsTxtFile method to parse that file
            if (URL == "/robots.txt")
            {
                return false;
            }
            else
            {
                foreach (string blockedURL in _BlockedUrls)
                {
                    if (URL.Length >= blockedURL.Length)
                    {
                        if (URL.Substring(0, blockedURL.Length) == blockedURL)
                        {
                            Log.Info("Blocked URL: " + blockedURL);
                            
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
