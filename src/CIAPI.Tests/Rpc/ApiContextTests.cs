﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CIAPI.DTO;
using CityIndex.JsonClient;
using CityIndex.JsonClient.Tests;
using NUnit.Framework;
using Client = CIAPI.Rpc.Client;

namespace CIAPI.Tests.Rpc
{
    [TestFixture]
    public class ApiContextTests
    {
        private readonly Dictionary<string, ICachingRequestQueue> _standardThrottleScopes = new Dictionary<string, ICachingRequestQueue>
                {
                    { "data", new RequestQueue (TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(5),30,10) }, 
                    { "trading", new RequestQueue (TimeSpan.FromMilliseconds(0),TimeSpan.FromSeconds(3),1,10) }
                };

        private const string NewsHeadlines12 = "{\"Headlines\":[{\"Headline\":\"(UK) Teenage girls often have babies fathered by men\",\"PublishDate\":\"\\/Date(1293727302736)\\/\",\"StoryId\":12654},{\"Headline\":\"(UK) Lung Cancer in Women Mushrooms\",\"PublishDate\":\"\\/Date(1293726702736)\\/\",\"StoryId\":12655},{\"Headline\":\"(UK) Include Your Children when Baking Cookies\",\"PublishDate\":\"\\/Date(1293726102736)\\/\",\"StoryId\":12656},{\"Headline\":\"(UK) Infertility unlikely to be passed on\",\"PublishDate\":\"\\/Date(1293725502736)\\/\",\"StoryId\":12657},{\"Headline\":\"(UK) Child's death ruins couple's holiday\",\"PublishDate\":\"\\/Date(1293724902736)\\/\",\"StoryId\":12658},{\"Headline\":\"(UK) Milk drinkers are turning to powder\",\"PublishDate\":\"\\/Date(1293724302736)\\/\",\"StoryId\":12659},{\"Headline\":\"(UK) Court Rules Boxer Shorts Are Indeed Underwear\",\"PublishDate\":\"\\/Date(1293723702736)\\/\",\"StoryId\":12660},{\"Headline\":\"(UK) Hospitals are Sued by 7 Foot Doctors\",\"PublishDate\":\"\\/Date(1293723102736)\\/\",\"StoryId\":12661},{\"Headline\":\"(UK) Lack of brains hinders research\",\"PublishDate\":\"\\/Date(1293722502736)\\/\",\"StoryId\":12662},{\"Headline\":\"(UK) New Vaccine May Contain Rabies\",\"PublishDate\":\"\\/Date(1293721902736)\\/\",\"StoryId\":12663},{\"Headline\":\"(UK) Two convicts evade noose, jury hung\",\"PublishDate\":\"\\/Date(1293721302736)\\/\",\"StoryId\":12664},{\"Headline\":\"(UK) Safety Experts Say School Bus Passengers Should Be Belted\",\"PublishDate\":\"\\/Date(1293720702736)\\/\",\"StoryId\":12665}]}";
        private const string NewsHeadlines14 = "{\"Headlines\":[{\"Headline\":\"(UK) Teenage girls often have babies fathered by men\",\"PublishDate\":\"\\/Date(1293727302736)\\/\",\"StoryId\":12654},{\"Headline\":\"(UK) Lung Cancer in Women Mushrooms\",\"PublishDate\":\"\\/Date(1293726702736)\\/\",\"StoryId\":12655},{\"Headline\":\"(UK) Include Your Children when Baking Cookies\",\"PublishDate\":\"\\/Date(1293726102736)\\/\",\"StoryId\":12656},{\"Headline\":\"(UK) Infertility unlikely to be passed on\",\"PublishDate\":\"\\/Date(1293725502736)\\/\",\"StoryId\":12657},{\"Headline\":\"(UK) Child's death ruins couple's holiday\",\"PublishDate\":\"\\/Date(1293724902736)\\/\",\"StoryId\":12658},{\"Headline\":\"(UK) Milk drinkers are turning to powder\",\"PublishDate\":\"\\/Date(1293724302736)\\/\",\"StoryId\":12659},{\"Headline\":\"(UK) Court Rules Boxer Shorts Are Indeed Underwear\",\"PublishDate\":\"\\/Date(1293723702736)\\/\",\"StoryId\":12660},{\"Headline\":\"(UK) Hospitals are Sued by 7 Foot Doctors\",\"PublishDate\":\"\\/Date(1293723102736)\\/\",\"StoryId\":12661},{\"Headline\":\"(UK) Lack of brains hinders research\",\"PublishDate\":\"\\/Date(1293722502736)\\/\",\"StoryId\":12662},{\"Headline\":\"(UK) New Vaccine May Contain Rabies\",\"PublishDate\":\"\\/Date(1293721902736)\\/\",\"StoryId\":12663},{\"Headline\":\"(UK) Two convicts evade noose, jury hung\",\"PublishDate\":\"\\/Date(1293721302736)\\/\",\"StoryId\":12664},{\"Headline\":\"(UK) Safety Experts Say School Bus Passengers Should Be Belted\",\"PublishDate\":\"\\/Date(1293720702736)\\/\",\"StoryId\":12665},{\"Headline\":\"(UK) Man Run Over by Freight Train Dies\",\"PublishDate\":\"\\/Date(1293720102736)\\/\",\"StoryId\":12666},{\"Headline\":\"(UK) Teenage girls often have babies fathered by men\",\"PublishDate\":\"\\/Date(1293727302736)\\/\",\"StoryId\":12654}]}";
        private const string BogusJson = "foo bar fu";
        private const string LoggedIn = "{\"Session\":\"D2FF3E4D-01EA-4741-86F0-437C919B5559\"}";
        private const string LoggedOut = "{\"LoggedOut\":true}";
        private const string AuthError = "{ \"ErrorMessage\": \"sample value\", \"ErrorCode\": 403 }";

        [Test]
        public void CanLogin()
        {
            Console.WriteLine("CanLogin");
            var ctx = BuildAuthenticatedClientAndSetupResponse(LoggedIn);

            CreateSessionResponseDTO response = ctx.CreateSession(TestConfig.ApiUsername, TestConfig.ApiPassword);

            Assert.IsNotNullOrEmpty(response.Session);

        }

        [Test, Ignore]
        public void ApiAuthenticationFailure()
        {
            var ctx = BuildAuthenticatedClientAndSetupResponse(LoggedIn);

            try
            {
                ctx.CreateSession("foo", "bar");
                Assert.Fail("Expected exception");
            }
            catch (ApiException ex)
            {
                Assert.AreEqual("[insert api unauthrized]", ex.Message, "FIXME: the API is just setting 401. it needs to send ErrorResponseDTO json as well.");
                Assert.AreEqual("[insert error response dto json]", ex.ResponseText);
            }
        }

        [Test]
        public void CanLogout()
        {
            Console.WriteLine("CanLogout");

            var ctx = BuildAuthenticatedClientAndSetupResponse(LoggedOut);

            SessionDeletionResponseDTO response = ctx.DeleteSession(TestConfig.ApiUsername, TestConfig.ApiTestSessionId);
            Assert.IsTrue(response.LoggedOut);
        }

        [Test]
        public void CanGetNewsHeadlines()
        {
            Console.WriteLine("CanGetNewsHeadlines");

            var ctx = BuildAuthenticatedClientAndSetupResponse(NewsHeadlines12);

            ListNewsHeadlinesResponseDTO response = ctx.ListNewsHeadlines("UK", 12);
            Assert.AreEqual(12, response.Headlines.Length);

        }


        [Test]
        public void DeserializationExceptionIsProperlySurfacedBySyncRequests()
        {
            Console.WriteLine("DeserializationExceptionIsProperlySurfacedBySyncRequests");
            CIAPI.Rpc.Client ctx = BuildAuthenticatedClientAndSetupResponse(BogusJson);
            Assert.Throws<CIAPI.Rpc.ServerConnectionException>(() => ctx.GetNewsDetail("foobar"));
        }


        [Test]
        public void DeserializationExceptionIsProperlySurfacedByAsyncRequests()
        {
            Console.WriteLine("DeserializationExceptionIsProperlySurfacedByAsyncRequests");

            CIAPI.Rpc.Client ctx = BuildAuthenticatedClientAndSetupResponse(BogusJson);

            var gate = new ManualResetEvent(false);
            ctx.BeginListNewsHeadlines("UK", 14, ar =>
                {
                    CIAPI.Rpc.Client innerCtx = (CIAPI.Rpc.Client)ar.AsyncState;
                    try
                    {
                        var response = innerCtx.EndListNewsHeadlines(ar);
                        Assert.Fail("expected exception");
                    }
                    catch (Exception ex)
                    {
                        Assert.IsNotNull(ex);
                        Assert.IsInstanceOf(
                            typeof(CIAPI.Rpc.ServerConnectionException), ex);
                    }
                    finally
                    {
                        gate.Set();
                    }
                }, ctx);

            gate.WaitOne(TimeSpan.FromSeconds(3));
        }

        [Test]
        public void CanGetNewsHeadlinesAsync()
        {
            Console.WriteLine("CanGetNewsHeadlinesAsync");

            CIAPI.Rpc.Client ctx = BuildAuthenticatedClientAndSetupResponse(NewsHeadlines14);

            var gate = new ManualResetEvent(false);
            ctx.BeginListNewsHeadlines("UK", 14, ar =>
                {
                    CIAPI.Rpc.Client innerCtx = (CIAPI.Rpc.Client)ar.AsyncState;
                    ListNewsHeadlinesResponseDTO response = innerCtx.EndListNewsHeadlines(ar);
                    Assert.AreEqual(14, response.Headlines.Length);
                    gate.Set();
                }, ctx);

            gate.WaitOne(TimeSpan.FromSeconds(3));
        }


        [Test]
        public void SpecificRequestExceptionsAreRetriedTheCorrectNumberOfTimes()
        {
            Console.WriteLine("SpecificRequestExceptionsAreRetriedTheCorrectNumberOfTimes");

            var gate = new ManualResetEvent(false);
            var requestFactory = new TestRequestFactory();

            const int EXPECTED_RETRY_COUNT = 2;

            var ctx = new Client(new Uri(TestConfig.RpcUrl), requestFactory,
                                           _standardThrottleScopes, EXPECTED_RETRY_COUNT)
                          {
                              UserName = TestConfig.ApiUsername,
                              SessionId = TestConfig.ApiTestSessionId
                          };

            requestFactory.CreateTestRequest(NewsHeadlines14, TimeSpan.FromMilliseconds(300), null, null,
                                             new WebException("(500) internal server error"));

            Exception exception = null;

            ctx.BeginListNewsHeadlines("UK", 14, ar =>
                                                     {
                                                         CIAPI.Rpc.Client innerCtx = (CIAPI.Rpc.Client)ar.AsyncState;
                                                         try
                                                         {
                                                             var response = innerCtx.EndListNewsHeadlines(ar);
                                                             Assert.Fail("Expected exception");
                                                         }
                                                         catch (Exception ex)
                                                         {
                                                             exception = ex;
                                                         }
                                                         finally
                                                         {
                                                             gate.Set();
                                                         }
                                                     }, ctx);
            gate.WaitOne(TimeSpan.FromSeconds(30));
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains(string.Format("(500) internal server error\r\nretried {0} times", EXPECTED_RETRY_COUNT)));
        }

        [Test]
        public void ShouldThrowExceptionIfRequestTimesOut()
        {
            var requestFactory = new TestRequestFactory
                                     {
                                         RequestTimeout = TimeSpan.FromSeconds(1)
                                     };

            var ctx = new Client(new Uri(TestConfig.RpcUrl), requestFactory, _standardThrottleScopes, 3);
            requestFactory.CreateTestRequest(LoggedIn, TimeSpan.FromSeconds(300));

            Assert.Throws<ApiException>(() => ctx.LogIn("foo", "bar"));
        }


        [Test]
        public void NonRetryableExceptionFailsInsteadOfRetrying()
        {
            Console.WriteLine("NonRetryableExceptionFailsInsteadOfRetrying");

            var requestFactory = new TestRequestFactory();

            var ctx = new Client(new Uri(TestConfig.RpcUrl), requestFactory, _standardThrottleScopes, 3);
            requestFactory.CreateTestRequest("", TimeSpan.FromMilliseconds(300), null, null, new WebException("(401) Unauthorized"));

            Assert.Throws<ApiException>(() => ctx.LogIn("foo", "bar"));
        }

        #region Plumbing

        private CIAPI.Rpc.Client BuildAuthenticatedClientAndSetupResponse(string expectedJson)
        {
            CIAPI.Rpc.Client ctx = BuildClientAndSetupResponse(expectedJson);

            ctx.UserName = TestConfig.ApiUsername;
            ctx.SessionId = TestConfig.ApiTestSessionId;

            return ctx;
        }

        private CIAPI.Rpc.Client BuildClientAndSetupResponse(string expectedJson)
        {
            var requestFactory = new TestRequestFactory();

            requestFactory.CreateTestRequest(expectedJson);

            var ctx = new CIAPI.Rpc.Client(new Uri(TestConfig.RpcUrl), requestFactory, _standardThrottleScopes, 3);
            return ctx;
        }
        #endregion
    }
}
