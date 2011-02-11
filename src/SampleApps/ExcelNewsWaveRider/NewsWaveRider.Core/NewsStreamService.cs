using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using NewsWaveRider.Core.ViewModels;

namespace NewsWaveRider.Core
{
    public class NewsStreamService
    {
        private List<string> _symbols;
        public event EventHandler<NewsEventArgs> NewNewsRecieved;

        private Timer _timer = new Timer(1000);
        public void Subscribe(List<string> symbols)
        {
            _symbols = symbols;
            _timer.Elapsed += (s, e) => OnMessage();
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void OnMessage()
        {
            var newsEvent = new NewsEvent()
            {
                PublishDate = DateTime.UtcNow,
                Market = "UK100",
                Headline = "UK 100 rises 1 point",
                Weight = 1
            };

            OnNewNewsRecieved(newsEvent);
        }

        private void OnNewNewsRecieved(NewsEvent newsEvent)
        {
            if (NewNewsRecieved == null) return;
            NewNewsRecieved(this, new NewsEventArgs(newsEvent));
        }
    }

    public class NewsEventArgs : EventArgs
    {
        public NewsEventArgs(NewsEvent newsEvent)
        {
            NewsEvent = newsEvent;
        }

        public NewsEvent NewsEvent { get; set; }
    }
}
