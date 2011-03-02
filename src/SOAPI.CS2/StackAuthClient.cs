﻿using System;
using System.Collections.Generic;
using System.Net;
using CityIndex.JsonClient;

namespace SOAPI.CS2
{
    /// <summary>
    /// http://stackauth.com/1.0/help/method?method=sites
    /// </summary>
    public partial class StackAuthClient : Client
    {
        
        #region cTor

        /// <summary>
        /// 
        /// </summary>
        public StackAuthClient()
            : base(
                new Uri("http://stackauth.com/1.0/"), new RequestFactory(),
                new Dictionary<string, ICachingRequestQueue> { { "", Throttle.Instance } }, 3)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cache"></param>
        /// <param name="requestFactory"></param>
        /// <param name="throttleScopes"></param>
        /// <param name="retryCount"></param>
        public StackAuthClient(Uri uri, IRequestFactory requestFactory,
                               Dictionary<string, ICachingRequestQueue> throttleScopes, int retryCount)
            : base(uri, requestFactory, throttleScopes, retryCount)
        {
            
        }

        #endregion


 


    }
}