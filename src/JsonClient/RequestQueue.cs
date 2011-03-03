using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Common.Logging;

namespace CityIndex.JsonClient
{
    ///<summary>
    ///</summary>
    public class RequestQueue : ICachingRequestQueue
    {
        private bool _disposed;

        public RequestQueue() : this(TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(5), 30, 10)
        {
        }

        public RequestQueue(TimeSpan defaultCacheDuration, TimeSpan throttleWindowTime, int throttleWindowCount, int maxPendingRequests)
        {
            _throttleWindowTime = throttleWindowTime;
            _throttleWindowCount = throttleWindowCount;
            _maxPendingRequests = maxPendingRequests;
            _defaultCacheDuration = defaultCacheDuration;
            _lock = new object();
            _items = new Dictionary<string, CacheItemBase>();

            _backgroundThread = new Thread(() =>
                                               {
                                                   while (true)
                                                   {
                                                       if (_disposed)
                                                       {
                                                           return;
                                                       }
                                                       lock(_lock)
                                                       {
                                                           // TODO: how/if handle exceptions?
                                                           PurgeExpiredItems(null);
                                                           ProcessQueue(null);    
                                                       }
                    
                                                       Thread.Sleep(100);
                                                   }
                                               });
            _backgroundThread.Start();
        }

        #region ICachingRequestQueue Members

        /// <summary>
        /// The number of requests that have been dispatched
        /// </summary>
        public int DispatchedCount
        {
            get { return _dispatchedCount; }
        }


        /// <summary>
        /// The maximum number of allowed pending request.
        /// 
        /// The throttle window will keep us in compliance with the 
        /// letter of the law, but testing has shown that a large 
        /// number of outstanding requests result in a cascade of 
        /// (500) errors that does not stop. 
        /// 
        /// So we will defer processing while there are > MaxPendingRequests 
        /// regardless of throttle window.
        /// </summary>
        public int MaxPendingRequests
        {
            get { return _maxPendingRequests; }
        }


        /// <summary>
        /// The number of pending (issued) requests
        /// </summary>
        public int PendingRequests
        {
            get { return _outstandingRequests; }
        }


        /// <summary>
        /// The quantitive portion (xxx) of the of 30 requests per 5 seconds
        /// </summary>
        public int ThrottleWindowCount
        {
            get { return _throttleWindowCount; }
        }


        /// <summary>
        /// The temporal portion (yyy) of the of 30 requests per 5 seconds
        /// </summary>
        public TimeSpan ThrottleWindowTime
        {
            get { return _throttleWindowTime; }
        }


        /// <summary>
        /// Adds a request to the end of the queue.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="action"></param>
        public void Enqueue(string url, WebRequest request, Action<IAsyncResult, RequestHolder> action)
        {
            lock (_requests)
            {
                // TODO: have a max queue length to keep things from getting out of hand - THEN we can throw an exception
                _requests.Enqueue(new RequestHolder
                                      {
                                          WebRequest = request,
                                          Url = url,
                                          AsyncResultHandler = action
                                      });
            }
        }

        /// <summary>
        /// Gets or creates a <see cref="CacheItem{TDTO}"/> for supplied url (case insensitive).
        /// If a matching <see cref="CacheItem{TDTO}"/> is found but has expired, it is replaced with a new <see cref="CacheItem{TDTO}"/>.
        /// </summary>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public CacheItem<TDTO> GetOrCreate<TDTO>(string url) where TDTO : class, new()
        {
            lock (_lock)
            {
                url = url.ToLower();

                EnsureItemCurrency(url);

                return _items.ContainsKey(url)
                           ? GetItem<TDTO>(url)
                           : CreateAndAddItem<TDTO>(url);
            }
        }


        /// <summary>
        /// Returns a <see cref="CacheItem{TDTO}"/> keyed by url (case insensitive)
        /// </summary>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">If url is not found in internal map</exception>
        public CacheItem<TDTO> Get<TDTO>(string url) where TDTO : class, new()
        {
            lock (_lock)
            {
                url = url.ToLower();
                if (_items.ContainsKey(url))
                {
                    return (CacheItem<TDTO>)_items[url];
                }
                throw new KeyNotFoundException("item for " + url + " was not found in the cache");
            }
        }

        /// <summary>
        /// Removes a <see cref="CacheItem{TDTO}"/> from the internal map
        /// </summary>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// If item is not completed, removing would result in orphaned callbacks effectively stalling the calling code.
        /// </exception>
        public CacheItem<TDTO> Remove<TDTO>(string url) where TDTO : class, new()
        {
            lock (_lock)
            {
                url = url.ToLower();
                CacheItem<TDTO> item = Get<TDTO>(url);
                if (item.ItemState != CacheItemState.Complete)
                {
                    throw new InvalidOperationException(
                        "Item is not completed. Removing would orphan asynchronous callbacks.");
                }
                _items.Remove(url);
                return item;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Private implementation

        /// <summary>
        /// Is called on the purge timer to remove completed and expired <see cref="CacheItem{TDTO}"/> from the internal map.
        /// </summary>
        /// <param name="ignored"></param>
        private void PurgeExpiredItems(object ignored)
        {
            lock (_lock)
            {
                var toRemove = new List<string>();

                foreach (var item in _items)
                {
                    if (item.Value.Expiration <= DateTimeOffset.UtcNow &&
                        item.Value.ItemState == CacheItemState.Complete)
                    {
                        toRemove.Add(item.Key);
                    }
                }

                foreach (string item in toRemove)
                {
                    _items.Remove(item);
                    Log.DebugFormat("Removed {0} from cache", item);
                }
            }
        }

        /// <summary>
        /// Creates and returns an empty <see cref="CacheItem{TDTO}"/> with default values and adds it to the internal map
        /// </summary>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        private CacheItem<TDTO> CreateAndAddItem<TDTO>(string url)
            where TDTO : class, new()
        {
            var item = new CacheItem<TDTO>
                           {
                               ItemState = CacheItemState.New,
                               CacheDuration = _defaultCacheDuration
                           };

            _items.Add(url, item);

            return item;
        }


        /// <summary>
        /// Fetches a <see cref="CacheItem{TDTO}"/> from internal map and blocks if 
        /// the <see cref="CacheItem{TDTO}"/> callbacks are being processed
        /// </summary>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        private CacheItem<TDTO> GetItem<TDTO>(string url) where TDTO : class, new()
        {
            var item = (CacheItem<TDTO>)_items[url];

            if (item.ItemState == CacheItemState.Processing)
            {
                // if currently processing callbacks we need to block
                item.ProcessingComplete += CacheItemProcessingComplete;
                item.ProcessingWaitHandle.WaitOne(); // TODO: timeout and throw if necessary
            }
            return item;
        }

        /// <summary>
        /// Signals processing complete on a <see cref="CacheItem{TDTO}"/> and cleans up the handler delegate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CacheItemProcessingComplete(object sender, EventArgs e)
        {
            var item = (CacheItemBase)sender;
            item.ProcessingComplete -= CacheItemProcessingComplete;
            item.ProcessingWaitHandle.Set();
        }

        /// <summary>
        /// Finds a <see cref="CacheItem{TDTO}"/> keyed by url (case insensitive), if found, is completed and expired, removes the item.
        /// </summary>
        /// <param name="url"></param>
        private void EnsureItemCurrency(string url)
        {
            bool itemIsExpired = _items.ContainsKey(url) && _items[url].ItemState == CacheItemState.Complete
                                 && _items[url].Expiration <= DateTimeOffset.UtcNow;

            if (itemIsExpired)
            {
                _items.Remove(url);
            }
        }

        #endregion

        private void ProcessQueue(object ignored)
        {
            lock (_requests)
            {
                if (_processingQueue) return;
                if (_requests.Count == 0) return;

                RequestHolder request = _requests.Peek();

                _processingQueue = true;

                try
                {
                    if (ThereAreMoreOutstandingRequestsThanIsAllowed()) return;

                    if (_requestTimes.Count > ThrottleWindowCount)
                    {
                        throw new Exception("request time queue got to be longer than window somehow");
                    }

                    if (_requestTimes.Count == ThrottleWindowCount)
                    {
                        DateTimeOffset head = _requestTimes.Peek();
                        TimeSpan waitTime = (ThrottleWindowTime - (DateTimeOffset.UtcNow - head));

                        if (waitTime.TotalMilliseconds > 0)
                        {
                            if (!_notifiedWaitingOnWindow)
                            {
                                string msgWaiting = string.Format("Waiting: " + waitTime + " to send " + request.Url);
                                Log.Debug(msgWaiting);

                                _notifiedWaitingOnWindow = true;
                            }
                            return;
                        }
                        _requestTimes.Dequeue();
                    }


                    // good to go. 
                    _notifiedWaitingOnWindow = false;

                    _requestTimes.Enqueue(DateTimeOffset.UtcNow);
                    _dispatchedCount += 1;

                    request.RequestIndex = _dispatchedCount;

                    try
                    {
                        IAsyncResult webRequestAsyncResult = request.WebRequest.BeginGetResponse(ar =>
                                                                                                     {
                                                                                                         Log.Debug(string.Format("Recieved #{0} : {1} ",request.RequestIndex,request.Url));

                                                                                                         _outstandingRequests--;

                                                                                                         request.AsyncResultHandler(ar,request);

                                                                                                     }, null);


                        EnsureRequestWillAbortAfterTimeout(request, webRequestAsyncResult);

                        Log.Debug(string.Format("Dispatched #{0} : {1} ", request.RequestIndex, request.Url));
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(string.Format("Error dispatching #{0} : {1} \r\n{2}", request.RequestIndex,
                                                request.Url, ex.Message));

                        throw;
                    }
                    finally
                    {
                        _requests.Dequeue();
                        _outstandingRequests++;
                    }
                }
                finally
                {
                    _processingQueue = false;
                }
            }
        }

        private bool ThereAreMoreOutstandingRequestsThanIsAllowed()
        {
            if (_outstandingRequests > MaxPendingRequests)
            {
                if (!_notifiedWaitingOnMaxPending)
                {
                    string msgMaxPending = string.Format("Waiting: pending requests {0}", _outstandingRequests);
                    Log.Debug(msgMaxPending);

                    _notifiedWaitingOnMaxPending = true;
                }

                return true;
            }

            _notifiedWaitingOnMaxPending = false;
            return false;
        }

        private void EnsureRequestWillAbortAfterTimeout(RequestHolder request, IAsyncResult result)
        {
            //TODO: How can we timeout a request for Silverlight, when calls to AsyncWaitHandle throw the following:
            //   Specified method is not supported. at System.Net.Browser.OHWRAsyncResult.get_AsyncWaitHandle() 

            // DAVID: i don't think that the async methods have a timeout parameter. we will need to build one into 
            // it. will not be terribly clean as it will prolly have to span both the throttle and the cache. I will look into it


#if !SILVERLIGHT
            ThreadPool.RegisterWaitForSingleObject(
                waitObject: result.AsyncWaitHandle,
                callBack: (state, isTimedOut) =>
                              {
                                  if (!isTimedOut) return;
                                  if (state.GetType() != typeof(RequestHolder)) return;

                                  var rh = (RequestHolder)state;
                                  Log.Error(string.Format("Aborting #{0} : {1} because it has exceeded timeout {2}",
                                                          rh.RequestIndex, rh.WebRequest.RequestUri, rh.RequestTimeout));
                                  rh.WebRequest.Abort();
                              },
                state: request,
                timeout: request.RequestTimeout,
                executeOnlyOnce: true);
#endif
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _disposing = true;
                    while (_backgroundThread.IsAlive)
                    {
                        Thread.Sleep(100);
                    }
                }

                _disposed = true;
            }
        }

        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(typeof(RequestQueue));
        private readonly Thread _backgroundThread;
        private readonly TimeSpan _defaultCacheDuration;
        private readonly Dictionary<string, CacheItemBase> _items;
        private readonly object _lock;
        private readonly int _maxPendingRequests;
        private readonly Queue<DateTimeOffset> _requestTimes = new Queue<DateTimeOffset>();
        private readonly Queue<RequestHolder> _requests = new Queue<RequestHolder>();

        private readonly int _throttleWindowCount;
        private readonly TimeSpan _throttleWindowTime;
        private int _dispatchedCount;
        private volatile bool _disposing;
        private bool _notifiedWaitingOnMaxPending;
        private bool _notifiedWaitingOnWindow;
        private int _outstandingRequests;
        private bool _processingQueue;

        #endregion
    }
}