using System;
using CityIndex.JsonClient;

namespace SOAPI.CS2
{
    public static class Throttle
    {
        static Throttle()
        {
            Instance = new RequestQueue(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(6), 25, 10);
        }

        public static ICachingRequestQueue Instance { get; set; }
    }
}