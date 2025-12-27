
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Product.Application.ViewModels;
using Shared.Events;
using Shared.Models;
using Shared.Services.Abstractions;
using System.Threading.Tasks;

namespace Product.Application.Controllers
{
    public class ProductController : Controller
    {
        private readonly IEventStoreService _eventStoreService;
        private readonly IMongoDbService _mongoDbService;

        public ProductController(IEventStoreService eventStoreService, IMongoDbService mongoDbService)
        {
            _eventStoreService = eventStoreService;
            _mongoDbService = mongoDbService;
        }

        public async Task<IActionResult> Index()
        {
            var productCollection = _mongoDbService.GetCollection<Shared.Models.Product>("Products");
            var products = await (await productCollection.FindAsync(_ => true)).ToListAsync();
            return View(products);
        }

        public IActionResult CreateProduct()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductVM createProductVM)
        {
            NewProductAddedEvent newProductAddedEvent = new()
            {
                ProductId = Guid.NewGuid().ToString(),
                ProductName = createProductVM.ProductName,
                Count = createProductVM.Count,
                IsAvailable = createProductVM.IsAvailable,
                Price = createProductVM.Price
            };
            await _eventStoreService.AppendToStreamAsync(
                streamName: "ProductStream",
                eventData: new[] { _eventStoreService.GenerateEventData(newProductAddedEvent) }
                );
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> UpdateProduct(string Id)
        {
            var productCollection = _mongoDbService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == Id)).FirstOrDefaultAsync();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> CountUpdate(Shared.Models.Product model, int status)
        {
            var productCollection = _mongoDbService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();

            if (status == 1)
            {
                CountDecreasedEvent countDecreasedEvent = new()
                {
                    ProductId = model.Id,
                    DecreaseAmount = model.Count
                };
                await _eventStoreService.AppendToStreamAsync(
                 streamName: "ProductStream",
                 eventData: new[] { _eventStoreService.GenerateEventData(countDecreasedEvent) }
                 );
            }
            else if (status == 0)
            {
                CountIncreasedEvent countIncreasedEvent = new()
                {
                    ProductId = model.Id,
                    IncreaseAmount = model.Count
                };
                await _eventStoreService.AppendToStreamAsync(
                    streamName: "ProductStream",
                    eventData: new[] { _eventStoreService.GenerateEventData(countIncreasedEvent) }
                    );
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PriceUpdate(Shared.Models.Product model,int status)
        {
            var productCollection = _mongoDbService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();
            if (status == 1)
            {
                PriceDecreasedEvent priceDecreasedEvent = new()
                {
                    ProductId = model.Id,
                    DecreasePrice = model.Price
                };

                await _eventStoreService.AppendToStreamAsync(
                    streamName: "ProductStream",
                    eventData: new[] { _eventStoreService.GenerateEventData(priceDecreasedEvent) }
                    );

            }
            else if (status ==  0)
            {
                PriceIncreaseEvent priceIncreaseEvent = new()
                {
                    ProductId = model.Id,
                    IncreasePrice = model.Price
                };
                await _eventStoreService.AppendToStreamAsync(
                    streamName: "ProductStream",
                    eventData: new[] { _eventStoreService.GenerateEventData(priceIncreaseEvent) }
                    );
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AvailableUpdate(Shared.Models.Product model)
        {
            var productCollection = _mongoDbService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == model.Id)).FirstOrDefaultAsync();

            if (product.IsAvailable != model.IsAvailable)
            {
                AvailabilityChangedEvent availabilityChangedEvent = new()
                {
                    ProductId = model.Id,
                    IsAvailable = model.IsAvailable
                };
                await _eventStoreService.AppendToStreamAsync(
                    streamName: "ProductStream",
                    eventData: new[] { _eventStoreService.GenerateEventData(availabilityChangedEvent) }
                    );

            }


            return RedirectToAction("Index");
        }

    }
}
