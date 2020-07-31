using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SimpleLP.Domain;

namespace SimpleLP
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // connect to API
            var key = Environment.GetEnvironmentVariable("HFT_API_KEY");
            var api = new ApiTrader("https://hft-apiv2-grpc.lykke.com", key);

            // create a bot with BTCUSD pair
            var trader = new MarketManager(api, "BTCUSD");

            // init trading bot: Current Price = 10950, Limit Order size = 0.0001 BTC, Delta(level spread) = 10 USD, count level per side = 15            
            trader.ResetMarke(10950, 0.0001m, 10, 15);
            
            // place Limit Orders
            await trader.PlaceToMarketAsync();

            while (true)
            {
                try
                {
                    // subscribe to trade stream
                    var tradeStream = api.Client.PrivateApi.GetTradeUpdates(new Empty());

                    // wait next trade event
                    while (await tradeStream.ResponseStream.MoveNext())
                    {
                        // handle each trade from event
                        foreach (var trade in tradeStream.ResponseStream.Current.Trades)
                        {
                            Console.WriteLine($"TRADE: orderId: {trade.OrderId}, size: {trade.BaseVolume}, role: {trade.Role}");
                            await trader.HandleTrade(trade.OrderId);
                        }

                        // place new Limit Orders
                        await trader.PlaceToMarketAsync();
                    }

                    tradeStream.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on stream read:");
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
