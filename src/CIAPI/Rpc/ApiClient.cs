﻿using System;
using System.Collections.Generic;
using System.Net;
using CIAPI.DTO;
using CityIndex.JsonClient;
using Newtonsoft.Json;

namespace CIAPI.Rpc
{
    public class ErrorResponseDTOJsonExceptionFactory : IJsonExceptionFactory
    {
        public Exception ParseException(string json)
        {
            return ParseException(null, json, null);
        }

        public Exception ParseException(string extraInfo, string json, Exception inner)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (json.Contains("\"ErrorMessage\"") && json.Contains("\"ErrorCode\""))
                {
                    try
                    {
                        // we cannot guarantee that for some strange reason a news item will not contain the strings
                        // searched for above so just need to try to parse the json and swallow
                        ErrorResponseDTO errorResponseDTO = JsonConvert.DeserializeObject<ErrorResponseDTO>(json);
                        ApiException ex = new ApiException(errorResponseDTO.ErrorMessage + (!string.IsNullOrEmpty(extraInfo) ? "\r\n" + extraInfo : ""));
                        ex.ResponseText = json;
                        return ex;
                    }
                    // ReSharper disable EmptyGeneralCatchClause

                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {


                    }
                }

            }
            return null;
        }
    }
    public partial class Client : CityIndex.JsonClient.Client
    {
        public Client(Uri uri)
            : base(uri, new RequestController(TimeSpan.FromSeconds(0), 2, new RequestFactory(), new ErrorResponseDTOJsonExceptionFactory(), new ThrottedRequestQueue(TimeSpan.FromSeconds(5), 30, 10, "data"), new ThrottedRequestQueue(TimeSpan.FromSeconds(3), 1, 3, "trading")))
        {
        }


        public Client(Uri uri, IRequestController requestController)
            : base(uri, requestController)
        {
        }


        public string UserName { get; set; }
        public string SessionId { get; set; }

        /// <summary>
        /// Authenticates the request with the API using request headers.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <param name="target"></param>
        /// <param name="uriTemplate"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="cacheDuration"></param>
        /// <param name="throttleScope"></param>
        protected override void BeforeIssueRequest(WebRequest request, string url, string target, string uriTemplate,
                                                   string method, Dictionary<string, object> parameters,
                                                   TimeSpan cacheDuration, string throttleScope)
        {
            if (url.IndexOf("/session", StringComparison.OrdinalIgnoreCase) == -1)
            {
                request.Headers["UserName"] = UserName;
                // API advertises session id as a GUID but treats as a string internally so we need to ucase here.
                if (SessionId == null)
                {
                    throw new ApiException("SessionId is null. Have you created a session? (logged in)");
                }
                request.Headers["Session"] = SessionId.ToUpper();
            }
        }

        #region AUthentication Wrapper

        /// <summary>
        /// Log In
        /// </summary>		
        /// <param name="userName">Username is case sensitive</param>
        /// <param name="password">Password is case sensitive</param>
        /// <returns></returns>
        public void LogIn(String userName, String password)
        {
            UserName = userName;
            SessionId = null;

            var response = Request<CreateSessionResponseDTO>("session", "/", "POST", new Dictionary<string, object>
                                                                                         {
                                                                                             {"UserName", userName},
                                                                                             {"Password", password},
                                                                                         }, TimeSpan.FromMilliseconds(0),
                                                             "data");
            SessionId = response.Session;
        }

        /// <summary>
        /// Log In
        /// </summary>		
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <param name="userName">Username is case sensitive</param>
        /// <param name="password">Password is case sensitive</param>
        /// <returns></returns>
        public void BeginLogIn(String userName, String password, ApiAsyncCallback<CreateSessionResponseDTO> callback,
                               object state)
        {
            UserName = userName;
            SessionId = null;

            BeginRequest(callback, state, "session", "/", "POST", new Dictionary<string, object>
                                                                      {
                                                                          {"UserName", userName},
                                                                          {"Password", password},
                                                                      }, TimeSpan.FromMilliseconds(0), "data");
        }

        public override TDTO EndRequest<TDTO>(ApiAsyncResult<TDTO> asyncResult)
        {
            try
            {
                TDTO response = base.EndRequest(asyncResult);
                return response;
            }
            catch (ApiSerializationException ex)
            {
                throw new ServerConnectionException(
                    "Invalid response received.  Are you connecting to the correct server Url?  See ResponseText property for further details of response received.",
                    ex.ResponseText);
            }
        }


        public void EndLogIn(ApiAsyncResult<CreateSessionResponseDTO> asyncResult)
        {
            CreateSessionResponseDTO response = EndRequest(asyncResult);
            SessionId = response.Session;
        }

        /// <summary>
        /// Log out
        /// </summary>		
        /// <returns></returns>
        public bool LogOut()
        {
            var response = Request<SessionDeletionResponseDTO>("session",
                                                               "/deleteSession?userName={userName}&session={session}",
                                                               "POST", new Dictionary<string, object>
                                                                           {
                                                                               {"userName", UserName},
                                                                               {"session", SessionId},
                                                                           }, TimeSpan.FromMilliseconds(0), "data");
            if (response.LoggedOut)
            {
                SessionId = null;
            }

            return response.LoggedOut;
        }

        /// <summary>
        /// Log out
        /// </summary>		
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public void BeginLogOut(ApiAsyncCallback<SessionDeletionResponseDTO> callback, object state)
        {
            BeginRequest(callback, state, "session", "/deleteSession?userName={userName}&session={session}", "POST",
                         new Dictionary<string, object>
                             {
                                 {"userName", UserName},
                                 {"session", SessionId},
                             }, TimeSpan.FromMilliseconds(0), "data");
        }

        public bool EndLogOut(ApiAsyncResult<SessionDeletionResponseDTO> asyncResult)
        {
            SessionDeletionResponseDTO response = EndRequest(asyncResult);

            if (response.LoggedOut)
            {
                SessionId = null;
            }

            return response.LoggedOut;
        }

        #endregion
    }


    public class ServerConnectionException : ApiSerializationException
    {
        public ServerConnectionException(string message, string responseText)
            : base(message, responseText)
        {
        }
    }
}