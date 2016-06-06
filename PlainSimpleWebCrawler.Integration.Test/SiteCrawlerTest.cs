using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlainSimpleWebCrawler.Integration.Test
{
    /// <summary>
    ///This is a test class for SiteCrawlerTest and is intended
    ///to contain all SiteCrawlerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SiteCrawlerTest
    {

        [TestMethod]
        public void SimpleCrawlerTest()
        {
            var simple = new SimpleCrawler("http://www.google.com/");
            var pages = simple.Run();
            Assert.IsTrue(pages.Count > 3, " Simple Crawler Test www.google.com");
        }
    }
}
