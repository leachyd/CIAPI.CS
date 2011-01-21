﻿using System;
using Common.Logging;
using Lightstreamer.DotNet.Client;

namespace CIAPI.Streaming.Lightstreamer
{
    public abstract class LightStreamerListener<TDto> : IStreamingListener<TDto>, IHandyTableListener
        where TDto : class
    {
        protected LightstreamerDtoConverter<TDto> MessageConverter;
        protected SimpleTableInfo TableInfo;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (LightStreamerListener<TDto>));
        protected readonly string Topic;
        private readonly LSClient _lsClient;
        private SubscribedTableKey _subscribedTableKey;

        protected LightStreamerListener(string topic, LSClient lsClient)
        {
            Topic = topic;
            _lsClient = lsClient;
        }

        public event EventHandler<MessageEventArgs<TDto>> MessageRecieved;

        protected abstract void BeforeStart();

        public void Start()
        {
            BeforeStart();
            _subscribedTableKey = _lsClient.SubscribeTable(TableInfo, this, false);
        }

        public  void Stop()
        {
             if (_subscribedTableKey!=null)
                _lsClient.UnsubscribeTable(_subscribedTableKey);
        }

        void IHandyTableListener.OnRawUpdatesLost(int itemPos, string itemName, int lostUpdates)
        {
            throw new NotImplementedException();
        }

        void IHandyTableListener.OnSnapshotEnd(int itemPos, string itemName)
        {
            throw new NotImplementedException();
        }

        void IHandyTableListener.OnUnsubscr(int itemPos, string itemName)
        {
            throw new NotImplementedException();
        }

        void IHandyTableListener.OnUnsubscrAll()
        {
            throw new NotImplementedException();
        }

        void IHandyTableListener.OnUpdate(int itemPos, string itemName, UpdateInfo update)
        {
            try
            {
                if (MessageRecieved == null) return;

                MessageRecieved(this, new MessageEventArgs<TDto>(Topic, MessageConverter.Convert(update)));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                // TODO: lightstreamer swallows errors thrown here - live with it or fix lightstreamer client code
                throw;
            }
        }
    }
}