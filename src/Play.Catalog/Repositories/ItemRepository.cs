using MongoDB.Driver;
using Play.Catalog.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Play.Catalog.Repositories
{
    public class ItemRepository
    {
        private const string collectionName = "items";

        private readonly IMongoCollection<Item> _dbCollection;

        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public ItemRepository()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");

            var database = mongoClient.GetDatabase("Catalog");

            _dbCollection = database.GetCollection<Item>(collectionName);
        }


        public async Task<IReadOnlyCollection<Item>> GetAllAsync()
        {
            return await _dbCollection.Find(filterBuilder.Empty).ToListAsync();
        }

        public async Task<Item> GetAsync(Guid id)
        {
            // get exisiting Item
            FilterDefinition<Item> filter = filterBuilder.Eq(x => x.Id, id);
            return await _dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Item entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await _dbCollection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Item entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // get exisiting Item
            FilterDefinition<Item> filter = filterBuilder.Eq(x => x.Id, entity.Id);

            await _dbCollection.ReplaceOneAsync(filter, entity);
        }

        public async Task RemoveAsync(Guid id)
        {
            // get exisiting Item
            FilterDefinition<Item> filter = filterBuilder.Eq(x => x.Id, id);
            await _dbCollection.DeleteOneAsync(filter);
        }

    }
}
