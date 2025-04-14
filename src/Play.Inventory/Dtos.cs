﻿using System;

namespace Play.Inventory
{
    public class Dtos
    {
        public record GrantItemDto(Guid UserId, Guid CatalogItemId, int Quantity);

        public record InventoryItemDto(Guid CatalogItemId, int Quantity, DateTimeOffset AcquiredDate);
    }
}
