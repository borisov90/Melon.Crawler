namespace Melon.Crawler.Helpers
{
    public class RobotCommand
    {
        public RobotCommand(string commandline)
        {
            int commandPosition = commandline.IndexOf('#');
            if (commandPosition == 0)
            {
                //the line is commented
                Command = "COMMENT";
            }
            else
            {
                // there is a comment further down the line
                if (commandPosition >= 0)
                {
                    commandline = commandline.Substring(0, commandPosition);
                }
                
                if (commandline.Length > 0)
                {
                    //splits line into command and url parts
                    string[] lineArray = commandline.Split(':');
                    Command = lineArray[0].Trim().ToLower();
                    if (lineArray.Length > 1)
                    {
                        // set appropriate property depending on command type
                        if (Command == "user-agent")
                        {
                            UserAgent = lineArray[1].Trim();
                        }
                        else
                        {
                            Url = lineArray[1].Trim();
                            // if the URL is a full URL e.g sitemaps then it will contain <a href=*> so will add to URL
                            if (lineArray.Length > 2)
                            {
                                Url += ":" + lineArray[2].Trim();
                            }
                        }

                    }
                }
            }
        }

        public string Command { get; }

        public string Url { get; } = string.Empty;

        public string UserAgent { get; } = string.Empty;
    }
}
