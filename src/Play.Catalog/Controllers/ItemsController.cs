using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Entities;
using Play.CatalogContracts;
using Play.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Play.Catalog.Controllers
{
    [Route("api/Catalog")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> _itemRepository;

        private readonly IPublishEndpoint _publishEndpoint;

        public ItemsController(IRepository<Item> itemRepository, IPublishEndpoint publishEndpoint)
        {
            _itemRepository = itemRepository;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Route("GetAllCatalogItems")]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            var items = (await _itemRepository.GetAllAsync()).Select(x => x.AsDto());

            return Ok(items);
        }

        [HttpGet]
        [Route("GetCatalogItemById")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await _itemRepository.GetAsync(id);

            if(item == null)
            {
                return NotFound();
            }

            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.Now,
            };

            await _itemRepository.CreateAsync(item);

            await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetByIdAsync), new {id = item.Id}, item);
        }

        [HttpPut("{id}")]
        public async  Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var exisitingItem = await _itemRepository.GetAsync(id);

            if(exisitingItem == null)
            {
                return NotFound();
            }

            exisitingItem.Name = updateItemDto.Name;
            exisitingItem.Description = updateItemDto.Description;
            exisitingItem.Price = updateItemDto.Price;

            await _itemRepository.UpdateAsync(exisitingItem);

            await _publishEndpoint.Publish(new CatalogItemUpdated(exisitingItem.Id, exisitingItem.Name, exisitingItem.Description));

            return Ok();
        }

        [HttpDelete("{id}")]
        public async  Task<IActionResult> Delete(Guid id)
        {
            var exisitingItem = await _itemRepository.GetAsync(id);

            if (exisitingItem == null)
            {
                return NotFound();
            }

            await _itemRepository.RemoveAsync(id);

            await _publishEndpoint.Publish(new CatalogItemDeleted(id));

            return Ok();
        }
    }
}
