using System;
using System.Collections.Generic;
using System.Net;

namespace CityIndex.JsonClient
{
    public interface IThrottleManager
    {
        IRequestCache Cache { get; }

    }

    public interface IRequestQueue
    {
        
    }
    ///<summary>
    ///</summary>
    public interface ICachingRequestQueue : IDisposable
    {
        /// <summary>
        /// The number of requests that have been dispatched
        /// </summary>
        int DispatchedCount { get; }


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
        int MaxPendingRequests { get; }

        /// <summary>
        /// The number of pending (issued) requests
        /// </summary>
        int PendingRequests { get; }

        /// <summary>
        /// The quantitive portion (xxx) of the of 30 requests per 5 seconds
        /// </summary>
        int ThrottleWindowCount { get; }

        /// <summary>
        /// The temporal portion (yyy) of the of 30 requests per 5 seconds
        /// </summary>
        TimeSpan ThrottleWindowTime { get; }


        /// <summary>
        /// Adds a request to the end of the queue.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="action"></param>
        void Enqueue(string url, WebRequest request, Action<IAsyncResult, RequestHolder> action);

        /// <summary>
        /// Gets or creates a <see cref="CacheItem{TDTO}"/> for supplied url (case insensitive).
        /// If a matching <see cref="CacheItem{TDTO}"/> is found but has expired, it is replaced with a new <see cref="CacheItem{TDTO}"/>.
        /// </summary>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        CacheItem<TDTO> GetOrCreate<TDTO>(string url) where TDTO : class, new();

        /// <summary>
        /// Returns a <see cref="CacheItem{TDTO}"/> keyed by url (case insensitive)
        /// </summary>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">If url is not found in internal map</exception>
        CacheItem<TDTO> Get<TDTO>(string url) where TDTO : class, new();

        /// <summary>
        /// Removes a <see cref="CacheItem{TDTO}"/> from the internal map
        /// </summary>
        /// <typeparam name="TDTO"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// If item is not completed, removing would result in orphaned callbacks effectively stalling the calling code.
        /// </exception>
        CacheItem<TDTO> Remove<TDTO>(string url) where TDTO : class, new();
    }


    /////<summary>
    ///// Describes a self throttling asynchronous request queue.
    /////</summary>
    //public interface IThrottedRequestQueue : IDisposable
    //{
    //    /// <summary>
    //    /// The number of requests that have been dispatched
    //    /// </summary>
    //    int DispatchedCount { get; }


    //    /// <summary>
    //    /// The maximum number of allowed pending request.
    //    /// 
    //    /// The throttle window will keep us in compliance with the 
    //    /// letter of the law, but testing has shown that a large 
    //    /// number of outstanding requests result in a cascade of 
    //    /// (500) errors that does not stop. 
    //    /// 
    //    /// So we will defer processing while there are > MaxPendingRequests 
    //    /// regardless of throttle window.
    //    /// </summary>
    //    int MaxPendingRequests { get; }

    //    /// <summary>
    //    /// The number of pending (issued) requests
    //    /// </summary>
    //    int PendingRequests { get; }

    //    /// <summary>
    //    /// The quantitive portion (xxx) of the of 30 requests per 5 seconds
    //    /// </summary>
    //    int ThrottleWindowCount { get;}

    //    /// <summary>
    //    /// The temporal portion (yyy) of the of 30 requests per 5 seconds
    //    /// </summary>
    //    TimeSpan ThrottleWindowTime { get; }


    //    /// <summary>
    //    /// Adds a request to the end of the queue.
    //    /// </summary>
    //    /// <param name="url"></param>
    //    /// <param name="request"></param>
    //    /// <param name="action"></param>
    //    void Enqueue(string url, WebRequest request, Action<IAsyncResult,RequestHolder> action);

    //}
}