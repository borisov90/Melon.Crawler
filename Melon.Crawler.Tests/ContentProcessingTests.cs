using Melon.Crawler.Helpers;
using Xunit;
using System.Linq;
using Moq;
using log4net;

namespace Melon.Crawler.Tests
{
    public class ContentProcessingTests
    {
        private Mock<ILog> logger = new Mock<ILog>();
        private Extractor extractor = new Extractor();
        private readonly string formatedAdress = "https://stackoverflow.com/";
        private readonly string unformattedAdress = "stackoverflow.com";
        private readonly string linkFromDifferentDomain = "https://twitter.com/stackoverflow";
        private readonly string linkFromSameDomain = "https://stackoverflow.com/questions/";


        [Fact]
        public void UnformattedLinkShouldBeBlank()
        {
            UrlFormatter _urlFormatter = new UrlFormatter(logger.Object);

            var formatedLink = _urlFormatter.FormatLink(unformattedAdress);

            Assert.True(formatedLink == string.Empty);
        }

        [Fact]
        public void LinkFromDifferentDomainShouldBeBlank()
        {
            UrlFormatter _urlFormatter = new UrlFormatter(logger.Object);

            var formatedLink = _urlFormatter.FormatLink(linkFromDifferentDomain, formatedAdress);

            Assert.True(formatedLink == string.Empty);
        }

        [Fact]
        public void LinkFromSameDomainShouldBeBlank()
        {
            UrlFormatter _urlFormatter = new UrlFormatter(logger.Object);

            var formatedLink = _urlFormatter.FormatLink(linkFromSameDomain, formatedAdress);

            Assert.True(formatedLink != string.Empty);
        }

        [Fact]
        public void ExtractorShouldExtractAHrefNodes()
        {
            Reader _reader = new Reader(logger.Object);

            var content = _reader.GetContentFromUrl(formatedAdress);
            var extractedHtml = extractor.ExtractHrefs(content);
            var overalNodesCount = extractedHtml.Count();
            var extractedHrefs = extractedHtml.Where(node => node.Name == "a").Count();

            Assert.True(overalNodesCount == extractedHrefs);
        }

    }
}
