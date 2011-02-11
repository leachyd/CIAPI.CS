using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CIAPI.Rpc;

namespace NewsWaveRider.Core
{
    public static class ServiceFactory
    {
        private static Client _rpcClient;
        public static CIAPI.Rpc.Client GetRpcClient()
        {
            if (_rpcClient==null)
            {
                _rpcClient = new Client(new Uri("http://ciapipreprod.cityindextest9.co.uk/TradingApi"));
                _rpcClient.LogIn("xx189949", "password");
            }
            return _rpcClient;
        }

        public static HistoricNewsService CreateHistoricNewsService()
        {
            return new HistoricNewsService(new CityIndexNewsRepository(GetRpcClient()), new TwitterNewsRepository());
        }

        public static NewsStreamService CeateNewsStreamService()
        {
            return new NewsStreamService();
        }
    }
}
