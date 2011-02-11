using System.Collections.Generic;
using NUnit.Framework;

namespace NewsWaveRider.Core.IntegrationTests
{
    [TestFixture]
    public class CityIndexNewsRepositoryFixture
    {
        [Test]
        public void CanGetRecentNews()
        {
            var newsRepo = new CityIndexNewsRepository(ServiceFactory.GetRpcClient());
            var news = newsRepo.GetNews(new List<string> {"UK100", "DOWJ"});
            Assert.That(news.Count,Is.GreaterThan(0));
        }
    }

    [TestFixture]
    public class TwitterewsRepositoryFixture
    {
        [Test]
        public void CanGetRecentTweets()
        {
            var newsRepo = new TwitterNewsRepository();
            var news = newsRepo.GetNews(new List<string> { "UK100", "DOWJ" });
            Assert.That(news.Count, Is.GreaterThan(0));
        }
    }
}
