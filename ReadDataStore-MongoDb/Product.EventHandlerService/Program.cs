using Product.EventHandlerService;
using Product.EventHandlerService.WorkerServices;
using Shared.Services;
using Shared.Services.Abstractions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<EventStoreWorkerService>();

builder.Services.AddSingleton<IEventStoreService, EventStoreService>();
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

var host = builder.Build();
host.Run();
