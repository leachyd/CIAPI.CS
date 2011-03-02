//using System;
//using System.Collections.Generic;
//using System.Threading;
//using Common.Logging;

//namespace CityIndex.JsonClient
//{
//    /// <summary>
//    /// A thread-safe, self purging cache of <see cref="CacheItem{TDTO}"/>
//    /// </summary>
//    public class RequestCache : IRequestCache
//    {
//        #region cTor

//        /// <summary>
//        /// Instantiates a <see cref="RequestCache"/> with default purge interval of 10 seconds and default cache duration of 0 milliseconds.
//        /// </summary>
//        public RequestCache()
//            : this(TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(0))
//        {
//        }

//        /// <summary>
//        /// Instantiates a <see cref="RequestCache"/> with supplied <paramref name="purgeInterval"/> and <paramref name="defaultCacheDuration"/>
//        /// </summary>
//        /// <param name="purgeInterval">How often to scan the cache and purge expired items.</param>
//        /// <param name="defaultCacheDuration">The default cache lifespan to apply to <see cref="CacheItem{TDTO}"/></param>
//        public RequestCache(TimeSpan purgeInterval, TimeSpan defaultCacheDuration)
//        {
            
//            _defaultCacheDuration = defaultCacheDuration;
//            _lock = new object();
//            _items = new Dictionary<string, CacheItemBase>();
//            _backgroundThread = new Thread(() =>
//            {


//                while (true)
//                {
//                    if (_disposing)
//                    {
//                        return;
//                    }

//                    // TODO: how/why/should we surface exceptions on purge?

//                    PurgeExpiredItems(null);

//                    Thread.Sleep(purgeInterval);
//                }

//            });

//            _backgroundThread.Start();
//            //new Timer(PurgeExpiredItems, null, TimeSpan.FromMilliseconds(10), purgeInterval);
//        }

//        #endregion

//    }


//}