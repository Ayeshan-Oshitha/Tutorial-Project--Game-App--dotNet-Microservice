using MassTransit;
using Play.CatalogContracts;
using Play.Common;
using Play.Inventory.Entities;
using System.Threading.Tasks;

namespace Play.Inventory.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public CatalogItemCreatedConsumer(IRepository<CatalogItem> catalogItemRepository)
        {
            _catalogItemRepository = catalogItemRepository;
        }
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            // We receive the message through context variable
            var message = context.Message;

            var item = await _catalogItemRepository.GetAsync(message.ItemId);

            if(item != null)
            {
                return;
            }

            // IF the item received is not in our local database, we have to create that
            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description,
            };

            await _catalogItemRepository.CreateAsync(item);
        }
    }
}
