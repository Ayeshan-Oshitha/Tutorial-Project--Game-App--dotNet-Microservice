using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Entities;
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

        private static int requestCounter = 0;

        public ItemsController(IRepository<Item> itemRepository)
        {
            _itemRepository = itemRepository;
        }

        [HttpGet]
        [Route("GetAllCatalogItems")]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {
            requestCounter++;
            Console.WriteLine($"Request {requestCounter} : Starting...");

            // This code intentionally simulates a timeout and failure scenario
            if ( requestCounter <= 4 )
            {
                Console.WriteLine($"Request {requestCounter} : Delaying...");
                await Task.Delay(TimeSpan.FromSeconds(10));
                Console.WriteLine($"Request {requestCounter} : 500 (Internal Server Error) ");
                return StatusCode(5000);
            }

            var items = (await _itemRepository.GetAllAsync()).Select(x => x.AsDto());

            Console.WriteLine($"Request {requestCounter} : 200 (Ok) ");
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

            return Ok();
        }
    }
}
