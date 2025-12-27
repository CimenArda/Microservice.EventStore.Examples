#region İnceleme
//using EventStore.Client;
//using System.Text.Json;

//string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";

//var settings = EventStoreClientSettings.Create(connectionString);
//var client = new EventStoreClient(settings);


//var orderPlacedEvent = new OrderPlacedEvent
//{
//    OrderId = "order-123",
//    TotalAmount = 250
//};

////while (true)
////{

////    EventData eventData = new(
////        eventId: Uuid.NewUuid(),
////        type: orderPlacedEvent.GetType().Name,
////        data: JsonSerializer.SerializeToUtf8Bytes(orderPlacedEvent));

////    await client.AppendToStreamAsync(
////        streamName: "orders-stream",
////        expectedState: StreamState.Any,
////        eventData: new[] { eventData });

////}

////var  events = client.ReadStreamAsync(
////    streamName: "orders-stream",
////    direction: Direction.Forwards,
////    revision: StreamPosition.Start);

////var data = await events.ToListAsync();

////Console.WriteLine();



//await client.SubscribeToStreamAsync(
//    streamName: "orders-stream",
//    start: FromStream.Start,
//    eventAppeared: async (subscription, resolvedEvent, cancellationToken) =>
//    {
//        var eventData = JsonSerializer.Deserialize<OrderPlacedEvent>(resolvedEvent.Event.Data.Span);
//        Console.WriteLine($"Order Received: {eventData.OrderId}, Amount: {eventData.TotalAmount}");
//    },
//    subscriptionDropped: (subscription, reason, exception) =>
//    {
//        Console.WriteLine($"Subscription dropped: {reason}");
//    });

//Console.ReadLine();



//class OrderPlacedEvent
//{
//    public string OrderId { get; set; }
//    public int TotalAmount { get; set; }

//}




#endregion

#region Bank-Bakiye Örnek

using EventStore.Client;
using System.Text.Json;


EventStoreService eventStoreService = new();

AccountCreatedEvent accountCreatedEvent = new()
{
    AccountId = "12345",
    CostumerId = "98765",
    StartBalance = 0,
    Date = DateTime.UtcNow.Date
};
MoneyDepositedEvent moneyDepositedEvent1 = new()
{
    AccountId = "12345",
    Amount = 1000,
    Date = DateTime.UtcNow.Date
};
MoneyDepositedEvent moneyDepositedEvent2 = new()
{
    AccountId = "12345",
    Amount = 500,
    Date = DateTime.UtcNow.Date
};
MoneyWithdrawnEvent moneyWithdrawnEvent = new()
{
    AccountId = "12345",
    Amount = 200,
    Date = DateTime.UtcNow.Date
};
MoneyDepositedEvent moneyDepositedEvent3 = new()
{
    AccountId = "12345",
    Amount = 50,
    Date = DateTime.UtcNow.Date
};
MoneyTransferedEvent moneyTransferredEvent1 = new()
{
    AccountId = "12345",
    Amount = 250,
    Date = DateTime.UtcNow.Date
};
MoneyTransferedEvent moneyTransferredEvent2 = new()
{
    AccountId = "12345",
    Amount = 150,
    Date = DateTime.UtcNow.Date
};
MoneyDepositedEvent moneyDepositedEvent4 = new()
{
    AccountId = "12345",
    Amount = 2000,
    Date = DateTime.UtcNow.Date
};

//await eventStoreService.AppendToStreamAsync(
//    streamName: $"costumer-{accountCreatedEvent.CostumerId}-stream",
//    new[]
//    {
//        eventStoreService.GenerateEventData(accountCreatedEvent),
//        eventStoreService.GenerateEventData(moneyDepositedEvent1),
//        eventStoreService.GenerateEventData(moneyDepositedEvent2),
//        eventStoreService.GenerateEventData(moneyWithdrawnEvent),
//        eventStoreService.GenerateEventData(moneyDepositedEvent3),
//        eventStoreService.GenerateEventData(moneyTransferredEvent1),
//        eventStoreService.GenerateEventData(moneyTransferredEvent2),
//        eventStoreService.GenerateEventData(moneyDepositedEvent4)
//    }

//    );

BalanceInfo balanceInfo = new();
await eventStoreService.SubscribeToStreamAsync(
    streamName: $"costumer-{accountCreatedEvent.CostumerId}-stream",
    async (ss,re,ct) =>
    {
        string eventType = re.Event.EventType;
        object @event = JsonSerializer.Deserialize(re.Event.Data.ToArray(),Type.GetType(eventType));
        switch (@event)
        {
            case AccountCreatedEvent e:
               balanceInfo.AccountId = e.AccountId;
                balanceInfo.Balance = e.StartBalance;
                break;
            case MoneyDepositedEvent e:
              balanceInfo.Balance += e.Amount;
                break;
            case MoneyWithdrawnEvent e:
              balanceInfo.Balance -= e.Amount;
                break;
            case MoneyTransferedEvent e:
           balanceInfo.Balance -= e.Amount;
                break;
                
        }

        await Console.Out.WriteLineAsync("**********Balance**********");
        await Console.Out.WriteLineAsync(JsonSerializer.Serialize(balanceInfo));
        await Console.Out.WriteLineAsync("**********Balance*****************");
        await Console.Out.WriteLineAsync("");
        await Console.Out.WriteLineAsync("");
    });

Console.ReadLine();


class EventStoreService
{
 EventStoreClientSettings GetEventStoreClientSettings(string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false") => EventStoreClientSettings.Create(connectionString);
  EventStoreClient Client { get => new EventStoreClient(GetEventStoreClientSettings()); }

    public async Task AppendToStreamAsync(string streamName,IEnumerable<EventData> eventData) => await Client.AppendToStreamAsync(
        streamName: streamName,
        eventData: eventData,
        expectedState: StreamState.Any);

    public EventData GenerateEventData(object @event)
    {
        return new EventData(
            eventId: Uuid.NewUuid(),
            type: @event.GetType().Name,
            data: JsonSerializer.SerializeToUtf8Bytes(@event));
    }


    public async Task SubscribeToStreamAsync(string streamName, Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> eventAppeared) => await Client.SubscribeToStreamAsync(
        
        streamName: streamName,
        start: FromStream.Start,
        eventAppeared: eventAppeared,
        subscriptionDropped: (subscription, reason, exception) =>
        {
            Console.WriteLine($"Subscription dropped: {reason}");
        });

}

class BalanceInfo
{
    public string AccountId { get; set; }
    public int Balance { get; set; }
}
class AccountCreatedEvent
{
    public string AccountId { get; set; }
    public string CostumerId { get; set; }
    public int StartBalance { get; set; }
    public DateTime Date { get; set; }
}
class MoneyDepositedEvent
{
    public string AccountId { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
}
class MoneyWithdrawnEvent
{
    public string AccountId { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
}
class MoneyTransferedEvent
{
    public string AccountId { get; set; }
    public string TargetAccountId { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
}





#endregion