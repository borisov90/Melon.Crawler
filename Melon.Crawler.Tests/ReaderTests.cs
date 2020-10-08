using Xunit;
using Melon.Crawler.Helpers;
using Moq;
using log4net;

namespace Tests
{
    public class ReaderTests
    {
        private Mock<ILog> logger = new Mock<ILog>();
        private readonly string defaultQueryAddress = "https://stackoverflow.com/";
        private readonly string forbiddenToCrawl = "https://stackoverflow.com/search/";
        private readonly string availableToCrawl = "https://stackoverflow.com/questions/";

        [Fact]
        public void ReaderReturnsContent()
        {
            var content = string.Empty;
            Reader _reader = new Reader(logger.Object);
            content = _reader.GetContentFromUrl(defaultQueryAddress);

            Assert.True(content != string.Empty);
        }

        [Fact]
        public void RobotsTxtIsFound()
        {
            var content = string.Empty;
            Reader _reader = new Reader(logger.Object);
            content = _reader.FindRobotsTxt(defaultQueryAddress);

            Assert.True(content != string.Empty);
        }

        [Fact]
        public void UrlIsUnavailableToCrawl()
        {
            Reader _reader = new Reader(logger.Object);
            _reader.ParseRobotsTxt(defaultQueryAddress);

            var isAvailableToCrawl = _reader.URLIsAllowed(forbiddenToCrawl);

            Assert.False(isAvailableToCrawl);
        }

        [Fact]
        public void UrlIsAvailableToCrawl()
        {
            Reader _reader = new Reader(logger.Object);

            _reader.ParseRobotsTxt(defaultQueryAddress);

            var isAvailableToCrawl = _reader.URLIsAllowed(availableToCrawl);

            Assert.True(isAvailableToCrawl);
        }
    }
}