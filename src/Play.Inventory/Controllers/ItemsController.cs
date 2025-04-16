 using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Client;
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
        private readonly IRepository<InventoryItem> _inventoryItemRepository;
        private readonly IRepository<CatalogItem> _catalogItemsRepository;

        public ItemsController(IRepository<InventoryItem> inventoryItemRepository, IRepository<CatalogItem> catalogItemsRepository)
        {
            _inventoryItemRepository = inventoryItemRepository;
            _catalogItemsRepository = catalogItemsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if(userId == null)
            {
                return BadRequest();
            }

            var inventoryItemEntities = await _inventoryItemRepository.GetAllAsync(x => x.UserId == userId);

            var itemIds = inventoryItemEntities.Select(x => x.CatalogItemId);

            var catalogItemEntities = await _catalogItemsRepository.GetAllAsync(x => itemIds.Contains(x.Id));

            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(x => x.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemDtos);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemDto grantItemDto)
        {
            var inventoryItem = await _inventoryItemRepository.GetAsync(x => x.UserId == grantItemDto.UserId && x.CatalogItemId == grantItemDto.CatalogItemId );

            if(inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemDto.CatalogItemId,
                    UserId = grantItemDto.UserId,
                    Quantity = grantItemDto.Quantity,
                    AcquiredDate = DateTimeOffset.Now
                };
                await _inventoryItemRepository.CreateAsync(inventoryItem);
            } 
            else 
            { 
                inventoryItem.Quantity = inventoryItem.Quantity + grantItemDto.Quantity;
                await _inventoryItemRepository.UpdateAsync(inventoryItem);
            }

            return Ok(inventoryItem);

        }

    }
}
