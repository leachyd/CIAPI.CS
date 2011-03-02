//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Threading;
//using Common.Logging;

//namespace CityIndex.JsonClient
//{
//    /// <summary>
//    /// 
//    /// A self throttling asynchronous request queue.
//    /// 
//    /// TODO: allow for pausing
//    /// would like to allow for clearing the queue but dependencies on the cache make
//    /// this a non starter. the only viable ways to that means is to merge throttle and cache.
//    /// this will have to be a builder type action in the calling class that can corelate the
//    /// queue items and the cache items and resolve the cache items that are cleared.
//    /// this will probably mean the introduction of a 'cancelled' CacheItemState
//    /// </summary>
//    public sealed class ThrottedRequestQueue : IThrottedRequestQueue
//    {

//        #region Fields


//        private volatile bool _disposing;
//        private readonly Thread _backgroundThread;

//        #endregion

//        #region Constructors

//        /// <summary>
//        /// Insantiates a <see cref="ThrottedRequestQueue"/> with default parameters.
//        /// throttleWindowTime = 5 seconds
//        /// throttleWindowCount = 30
//        /// maxPendingRequests = 10
//        /// </summary>
//        public ThrottedRequestQueue()
//            : this(TimeSpan.FromSeconds(5), 30, 10)
//        {
//        }

//        /// <summary>
//        /// Insantiates a <see cref="ThrottedRequestQueue"/> with supplied parameters.
//        /// </summary>
//        /// <param name="throttleWindowTime">The window in which to restrice issued requests to <paramref name="throttleWindowCount"/></param>
//        /// <param name="throttleWindowCount">The maximum number of requests to issue in the amount of time described by <paramref name="throttleWindowTime"/></param>
//        /// <param name="maxPendingRequests">The maximum allowed number of active requests.</param>
//        public ThrottedRequestQueue(TimeSpan throttleWindowTime, int throttleWindowCount, int maxPendingRequests)
//        {
//            _throttleWindowTime = throttleWindowTime;
//            _throttleWindowCount = throttleWindowCount;
//            _maxPendingRequests = maxPendingRequests;
//            _backgroundThread = new Thread(() =>
//                                               {
//                                                   while (true)
//                                                   {
//                                                       if (_disposed)
//                                                       {
//                                                           return;
//                                                       }
//                                                       // TODO: how/if handle exceptions?
//                                                       ProcessQueue(null);
//                                                       Thread.Sleep(100);
//                                                   }
//                                               });
//            _backgroundThread.Start();
            
//        }

//        #endregion

//    }
//}