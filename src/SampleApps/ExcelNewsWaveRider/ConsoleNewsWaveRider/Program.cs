using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewsWaveRider.Core;

namespace ConsoleNewsWaveRider
{
    class Program
    {
        static void Main(string[] args)
        {
            var newsRepo = new TwitterNewsRepository();
            var news = newsRepo.GetNews(new List<string> { "UK100", "DOWJ" });
            foreach (var newsEvent in news)
            {
                Console.WriteLine(newsEvent.Headline);
            }
            Console.ReadKey();
        }
    }
}
