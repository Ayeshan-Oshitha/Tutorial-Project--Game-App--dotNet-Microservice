using MassTransit;
using Play.CatalogContracts;
using Play.Common;
using Play.Inventory.Entities;
using System.Threading.Tasks;

namespace Play.Inventory.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
    {
        private readonly IRepository<CatalogItem> _catalogItemRepository;

        public CatalogItemDeletedConsumer(IRepository<CatalogItem> catalogItemRepository)
        {
            _catalogItemRepository = catalogItemRepository;
        }
        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            // We receive the message through context variable
            var message = context.Message;

            var item = await _catalogItemRepository.GetAsync(message.ItemId);

            if(item == null)
            {
                return;
            }
            else
            {
               await _catalogItemRepository.RemoveAsync(message.ItemId);
            }
            
            
        }
    }
}
