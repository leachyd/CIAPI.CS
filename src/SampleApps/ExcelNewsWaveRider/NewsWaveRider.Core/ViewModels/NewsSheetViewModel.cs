using System;
using System.Collections.Generic;

namespace NewsWaveRider.Core.ViewModels
{
    public class NewsSheetViewModel
    {
        public NewsSheetViewModel()
        {
            NewsEvents = new List<NewsEvent>();
            NewsWaves = new List<NewsWave>();
        }

        public List<NewsEvent> NewsEvents { get; set; }
        public List<NewsWave> NewsWaves { get; set; }

        public void Init()
        {
            var symbols = new List<string>{"UK100","VOD.L","DOWJ"};
            foreach (var newsEvent in ServiceFactory.CreateHistoricNewsService().GetNews(symbols, TimeSpan.FromHours(4)))
            {
                AddNewsEvent(newsEvent);
            }

            OnDataUpdated();

            var newsStreamService = ServiceFactory.CeateNewsStreamService();
            newsStreamService.NewNewsRecieved += (sender, args) =>
                                                     {
                                                         AddNewsEvent(args.NewsEvent);
                                                         OnDataUpdated();
                                                     };
            newsStreamService.Subscribe(symbols);
            
        }

        private void AddNewsEvent(NewsEvent newsEvent)
        {
            NewsEvents.Add(newsEvent);
            var wave = NewsWaves.Find(newsWave => newsWave.Market == newsEvent.Market);
            if (wave == null)
            {
                wave = new NewsWave {Market = newsEvent.Market, MarketName = newsEvent.Market, Score = newsEvent.Weight};
                NewsWaves.Add(wave);
            }
            else
            {
                wave.Score += newsEvent.Weight;
            }
        }

        public event EventHandler DataUpdated;
        private void OnDataUpdated()
        {
            if (DataUpdated!=null)
                DataUpdated(this,new EventArgs());
        }
    }
}
