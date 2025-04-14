using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using static Play.Inventory.Dtos;

namespace Play.Inventory.Controllers
{
    [Route("api/Inventory")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _itemRepository;

        public ItemsController(IRepository<InventoryItem> itemRepository)
        {
            _itemRepository = itemRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if(userId == null)
            {
                return BadRequest();
            }

            var items = (await _itemRepository.GetAllAsync(x => x.UserId == userId)).Select(item => item.AsDto());

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemDto grantItemDto)
        {
            var inventoryItem = await _itemRepository.GetAsync(x => x.UserId == grantItemDto.UserId && x.CatalogItemId == grantItemDto.CatalogItemId );

            if(inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemDto.CatalogItemId,
                    UserId = grantItemDto.UserId,
                    Quantity = grantItemDto.Quantity,
                    AcquiredDate = DateTimeOffset.Now
                };
                await _itemRepository.CreateAsync(inventoryItem);
            } 
            else 
            { 
                inventoryItem.Quantity = inventoryItem.Quantity + grantItemDto.Quantity;
                await _itemRepository.UpdateAsync(inventoryItem);
            }

            return Ok(inventoryItem);

        }

    }
}
