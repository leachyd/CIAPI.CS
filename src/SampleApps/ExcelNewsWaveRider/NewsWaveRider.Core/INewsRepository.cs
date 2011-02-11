using System.Collections.Generic;
using NewsWaveRider.Core.ViewModels;

namespace NewsWaveRider.Core
{
    public interface INewsRepository
    {
        List<NewsEvent> GetNews(List<string> symbols);
    }
}