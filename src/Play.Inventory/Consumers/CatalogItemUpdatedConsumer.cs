using MassTransit;
using Play.CatalogContracts;
using Play.Common;
using Play.Inventory.Entities;
using System.Threading.Tasks;

namespace Play.Inventory.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> catalogItemRepository)
        {
          _catalogItemRepository = catalogItemRepository;
        }
        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            // We receive the message through context variable
            var message = context.Message;

            var item = await _catalogItemRepository.GetAsync(message.ItemId);

            if(item == null)
            {
                item = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description,
                };

                await _catalogItemRepository.CreateAsync(item);
            }
            else
            {
                item.Name = message.Name;
                item.Description = message.Description;

                await _catalogItemRepository.UpdateAsync(item);
            }
            
            
        }
    }
}
