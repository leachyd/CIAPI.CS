﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CIAPI.DTO;

using NUnit.Framework;

namespace CityIndex.JsonClient.Tests
{
    public class FooDTO { }
    [TestFixture]
    public class CacheFixture
    {
        [Test]
        public void ItemCanBeCached()
        {
            var c = new RequestQueue(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(10), 10, 30);
            lock (c)
            {
                var item = c.GetOrCreate<FooDTO>("foo");
                item.Expiration = DateTimeOffset.UtcNow.AddSeconds(2);
                item.ItemState = CacheItemState.Complete;

                new AutoResetEvent(false).WaitOne(1000);
                var actual = c.Get<FooDTO>("foo");
                Assert.IsNotNull(actual);
            }

        }

        [Test, ExpectedException(ExpectedMessage = "item for foo was not found in the cache")]
        public void ItemCanExpireAndBePurged()
        {
            var c = new RequestQueue(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(10), 10, 30);
            lock (c)
            {
                var item = c.GetOrCreate<FooDTO>("foo");
                item.Expiration = DateTimeOffset.UtcNow.AddSeconds(1);
                item.ItemState = CacheItemState.Complete;

                new AutoResetEvent(false).WaitOne(3000);
                c.Get<FooDTO>("foo");
                Assert.Fail("Expected exception");
            }
        }

    }
}
