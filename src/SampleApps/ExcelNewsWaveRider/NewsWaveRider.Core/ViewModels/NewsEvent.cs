using System;

namespace NewsWaveRider.Core.ViewModels
{
    public class NewsEvent
    {
        public DateTime PublishDate { get; set; }
        public string Market { get; set; }
        public int Weight { get; set; }
        public string Headline { get; set; }
    }
}