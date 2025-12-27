using MongoDB.Driver;
using Shared.Events;
using Shared.Models;
using Shared.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Product.EventHandlerService.WorkerServices
{
    public class EventStoreWorkerService : BackgroundService
    {
        private readonly IEventStoreService _eventStoreService;
        private readonly IMongoDbService _mongoDbService;
        public EventStoreWorkerService(IEventStoreService eventStoreService, IMongoDbService mongoDbService)
        {
            _eventStoreService = eventStoreService;
            _mongoDbService = mongoDbService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _eventStoreService.SubscribeToStreamAsync("ProductStream", async (streamSubscription, resolvedEvent, cancellationToken) =>
            {
                string eventType = resolvedEvent.Event.EventType;
                object @event = JsonSerializer.Deserialize(resolvedEvent.Event.Data.ToArray(),
                 Assembly.Load("Shared").GetTypes().FirstOrDefault(t => t.Name == eventType));

                var productCollection = _mongoDbService.GetCollection<Shared.Models.Product>("Products");

                Shared.Models.Product? product = null;

                switch (@event)
                {
                    case NewProductAddedEvent e:
                        var hasProduct = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).AnyAsync();
                        if (!hasProduct)
                        {
                            await productCollection.InsertOneAsync(new()
                            {
                                Id = e.ProductId,
                                ProductName = e.ProductName,
                                Count = e.Count,
                                IsAvailable = e.IsAvailable,
                                Price = e.Price
                            });
                        }

                        break;

                    case CountDecreasedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.Count -= e.DecreaseAmount;
                            await productCollection.ReplaceOneAsync(p => p.Id == product.Id, product);
                        }
                        break;
                    case CountIncreasedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.Count += e.IncreaseAmount;
                            await productCollection.ReplaceOneAsync(p => p.Id == product.Id, product);
                        }
                        break;
                    case PriceDecreasedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.Price -= e.DecreasePrice;
                            await productCollection.ReplaceOneAsync(p => p.Id == product.Id, product);
                        }
                        break;
                    case PriceIncreaseEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.Price += e.IncreasePrice;
                            await productCollection.ReplaceOneAsync(p => p.Id == product.Id, product);
                        }
                        break;
                    case AvailabilityChangedEvent e:
                        product = await (await productCollection.FindAsync(p => p.Id == e.ProductId)).FirstOrDefaultAsync();
                        if (product != null)
                        {
                            product.IsAvailable = e.IsAvailable;
                            await productCollection.ReplaceOneAsync(p => p.Id == product.Id, product);
                        }
                        break;


                }

            });
            


        }
    }
}
