# Example of usage Lykke Trading API with C#

Lykke Trading API documentation: [https://lykkecity.github.io/Trading-API/](https://lykkecity.github.io/Trading-API/)

## Client library

To build client library in `dotnet core 3` need to add .proto files into the project and configure autogeneration for gRPC.

Details you can see here: [TradingApi.Client](https://github.com/LykkeCity/Lykke-TradingAPI-Examples/tree/master/src/TradingApi.Client)

In class [TradingApiClient](https://github.com/LykkeCity/Lykke-TradingAPI-Examples/blob/master/src/TradingApi.Client/TradingApiClient.cs#L28) you can see how to create gRPC channel with api key. And create a PublicApi client and a PrivateApi client.


## Example: How to follow prices

source code:[Example.FollowPrices](https://github.com/LykkeCity/Lykke-TradingAPI-Examples/blob/master/src/Example.FollowPrices/Program.cs)

Create a client. You can skip the API key because you need just public API.

```csharp
  var client = new TradingApiClient("https://hft-apiv2-grpc.lykke.com", "");
```

Get prices snapshot

```csharp
  var priceRequest = new PricesRequest();
  
  // for example add filter by 3 instrument. You can keep AssetPairIds empty to receive all prices
  priceRequest.AssetPairIds.Add("BTCUSD");
  priceRequest.AssetPairIds.Add("BTCCHF");
  priceRequest.AssetPairIds.Add("BTCEUR");

  var prices = await client.PublicApi.GetPricesAsync(priceRequest);
  
  if (prices.error == null)
  {
    foreach(var price in prices.Payload)
    {
      Console.WriteLine($"{price.AssetPairId}: Ask={price.Ask}; Bid={price.Bid}; Time={price.Timestamp}");
    }
  }
  else
  {
    Console.WriteLine($"ERROR: {prices.error.Code}: {prices.error.Message}");
  }
```

Subscribe to price stream

```csharp
  var priceUpdateRequest = new PriceUpdatesRequest();
  priceUpdateRequest.AssetPairIds.Add("BTCUSD");
  priceUpdateRequest.AssetPairIds.Add("BTCCHF");
  priceUpdateRequest.AssetPairIds.Add("BTCEUR");

  Console.WriteLine("Subscribe to prices.");
  var priceStream = client.PublicApi.GetPriceUpdates(priceUpdateRequest);

  var token = new CancellationToken();
  while (await priceStream.ResponseStream.MoveNext(token))
  {
    var price = priceStream.ResponseStream.Current;

    Console.WriteLine($"{price.AssetPairId}: Ask={price.Ask}; Bid={price.Bid}; Time={price.Timestamp}");
  }
```
  
## Example: How to make a trader bot

In this example, you can see how to work simple trading bot. The idea of bot - provide liquidity to several sell levels and several buy levels. If an order is matched then trading bot should create a new order to compensate previous. A new order created with spread = Delta. In this case, if the market goes up and down then trading boot makes a `Profit from each level = LevelVolume * Delta`.

We need to:
1. Create a Limit Order
2. Cancel Limit Order
3. Get Limit Order status
4. Subscribe to trade stream by account

Implementation of trading boot you can see here: [SimpleLP](https://github.com/LykkeCity/Lykke-TradingAPI-Examples/blob/master/src/SimpleLP/Program.cs)
  
