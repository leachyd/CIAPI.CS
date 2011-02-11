using System;
using System.Collections.Generic;
using CIAPI.DTO;
using CIAPI.Rpc;
using NewsWaveRider.Core.ViewModels;

namespace NewsWaveRider.Core
{
    public class CityIndexNewsRepository : INewsRepository
    {
        private readonly Client _rpcClient;

        public CityIndexNewsRepository(CIAPI.Rpc.Client rpcClient)
        {
            _rpcClient = rpcClient;
        }

        public List<NewsEvent> GetNews(List<string> symbols)
        {
            var newsEvents = new List<NewsEvent>();

            var cityindexNews  = _rpcClient.ListNewsHeadlines("UK", 500);

//            var cityindexNews = new List<NewsDTO>{
//                                                     new NewsDTO{ PublishDate = DateTime.UtcNow, Headline = "A news headline about UK100", StoryId=376125},
//                                                     new NewsDTO{ PublishDate = DateTime.UtcNow, Headline = "A news headline about VOD.L", StoryId=376125},
//                                                     new NewsDTO{ PublishDate = DateTime.UtcNow, Headline = "A news headline about UK100", StoryId=376125},
//                                                     new NewsDTO{ PublishDate = DateTime.UtcNow, Headline = "A news headline about DOWJ", StoryId=376125},
//                                                     new NewsDTO{ PublishDate = DateTime.UtcNow, Headline = "A news headline about UK100", StoryId=376125},
//                                                     new NewsDTO{ PublishDate = DateTime.UtcNow, Headline = "A news headline about BP", StoryId=376125},
//                                                     new NewsDTO{ PublishDate = DateTime.UtcNow, Headline = "A news headline about VOD.L", StoryId=376125}
//                                                 };

            foreach (var newsDTO in cityindexNews.Headlines)
            {
                foreach (var symbol in symbols)
                {
                    if (!newsDTO.Headline.Contains(symbol))
                    {
                        var newsEvent = new NewsEvent()
                                            {
                                                PublishDate = newsDTO.PublishDate,
                                                Market = symbol,
                                                Headline = newsDTO.Headline,
                                                Weight = 1
                                            };
                        newsEvents.Add(newsEvent);
                    }
                }
            }

            return newsEvents;
        }
    }
}