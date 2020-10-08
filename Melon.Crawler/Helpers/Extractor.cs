using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Melon.Crawler.Helpers
{
    public class Extractor
    {
        public List<HtmlNode> ExtractHrefs(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var hrefs = htmlDocument.DocumentNode.SelectNodes("//a[@href]").ToList();
            return hrefs;
        }
    }
}
