using System;
using System.Collections.Generic;
using NewsWaveRider.Core.ViewModels;

namespace NewsWaveRider.Core
{
    public class HistoricNewsService
    {
        private readonly INewsRepository _cityIndexNewsRepository;
        private readonly INewsRepository _twitterNewsRepository;

        public HistoricNewsService(INewsRepository cityIndexNewsRepository, INewsRepository twitterNewsRepository)
        {
            _cityIndexNewsRepository = cityIndexNewsRepository;
            _twitterNewsRepository = twitterNewsRepository;
        }

        public List<NewsEvent> GetNews(List<string> symbols, TimeSpan inLast)
        {
            var relevantNewsEvents = new List<NewsEvent>();

            relevantNewsEvents.AddRange(_cityIndexNewsRepository.GetNews(symbols));
            relevantNewsEvents.AddRange(_twitterNewsRepository.GetNews(symbols));

            return relevantNewsEvents;
        }

    }
}
