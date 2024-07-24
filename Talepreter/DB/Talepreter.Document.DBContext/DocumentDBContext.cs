using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Talepreter.Common;

namespace Talepreter.Document.DBContext
{
    public class DocumentDBContext : IDocumentDBContext
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public DocumentDBContext()
        {
            var connectionString = EnvironmentVariableHandler.ReadEnvVar("MongoDBConnection");
            var dbName = EnvironmentVariableHandler.ReadEnvVar("MongoDBName");

            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(dbName);

            ConventionRegistry.Register(nameof(ImmutableTypeClassMapConvention), new ConventionPack { new ImmutableTypeClassMapConvention() }, type => true);
        }

        public async Task Setup()
        {
            await SetupIndexes<Actor>(DocumentDBStructure.Collections.Actors);
            await SetupIndexes<ActorTrait>(DocumentDBStructure.Collections.ActorTraits);
            await SetupIndexes<Anecdote>(DocumentDBStructure.Collections.Anecdotes);
            await SetupIndexes<Person>(DocumentDBStructure.Collections.Persons);
            await SetupIndexes<Page>(DocumentDBStructure.Collections.Pages);
            await SetupIndexes<Chapter>(DocumentDBStructure.Collections.Chapters);
            await SetupIndexes<Settlement>(DocumentDBStructure.Collections.Settlements);
            await SetupIndexes<World>(DocumentDBStructure.Collections.Worlds);
        }

        private async Task SetupIndexes<T>(string name) where T : EntityBase
        {
            await _database.CreateCollectionAsync(name);
            IMongoCollection<T> collection = _database.GetCollection<T>(name);

            var builder = Builders<T>.IndexKeys;
            var def = new CreateIndexModel<T>(builder.Ascending(a => a.TaleId), new CreateIndexOptions { Name = "idx_tale"});
            await collection.Indexes.CreateOneAsync(def);

            def = new CreateIndexModel<T>(builder.Combine(builder.Ascending(a => a.TaleId), builder.Ascending(a => a.TaleVersionId), builder.Ascending(a => a.Id)), new CreateIndexOptions { Name = "idx_tale_version_id" });
            await collection.Indexes.CreateOneAsync(def);
        }

        // --

        public async Task Put<T>(T document, CancellationToken token) where T: EntityBase
        {
            ArgumentNullException.ThrowIfNull(document, nameof(document));
            var collection = _database.GetCollection<T>(document.CollectionName);
            await collection.InsertOneAsync(document, null!, token);
        }

        public async Task PurgeTale(Guid taleId, CancellationToken token)
        {
            await DeleteMany<Actor>(taleId, DocumentDBStructure.Collections.Actors, token);
            await DeleteMany<ActorTrait>(taleId, DocumentDBStructure.Collections.ActorTraits, token);
            await DeleteMany<Anecdote>(taleId, DocumentDBStructure.Collections.Anecdotes, token);
            await DeleteMany<Person>(taleId, DocumentDBStructure.Collections.Persons, token);
            await DeleteMany<Page>(taleId, DocumentDBStructure.Collections.Pages, token);
            await DeleteMany<Chapter>(taleId, DocumentDBStructure.Collections.Chapters, token);
            await DeleteMany<Settlement>(taleId, DocumentDBStructure.Collections.Settlements, token);
            await DeleteMany<World>(taleId, DocumentDBStructure.Collections.Worlds, token);
        }

        public async Task PurgePublish(Guid taleId, Guid taleVersionId, CancellationToken token)
        {
            await DeleteMany<Actor>(taleId, taleVersionId, DocumentDBStructure.Collections.Actors, token);
            await DeleteMany<ActorTrait>(taleId, taleVersionId, DocumentDBStructure.Collections.ActorTraits, token);
            await DeleteMany<Anecdote>(taleId, taleVersionId, DocumentDBStructure.Collections.Anecdotes, token);
            await DeleteMany<Person>(taleId, taleVersionId, DocumentDBStructure.Collections.Persons, token);
            await DeleteMany<Page>(taleId, taleVersionId, DocumentDBStructure.Collections.Pages, token);
            await DeleteMany<Chapter>(taleId, taleVersionId, DocumentDBStructure.Collections.Chapters, token);
            await DeleteMany<Settlement>(taleId, taleVersionId, DocumentDBStructure.Collections.Settlements, token);
            await DeleteMany<World>(taleId, taleVersionId, DocumentDBStructure.Collections.Worlds, token);
        }

        // --

        private async Task DeleteMany<E>(Guid taleId, string name, CancellationToken token) where E: EntityBase
        {
            var collection = _database.GetCollection<E>(name);
            var filter = Builders<E>.Filter.Eq(t => t.TaleId, taleId);
            await collection.DeleteManyAsync(filter, token);
        }

        private async Task DeleteMany<E>(Guid taleId, Guid taleVersionId, string name, CancellationToken token) where E : EntityBase
        {
            var collection = _database.GetCollection<E>(name);
            var filter = Builders<E>.Filter.And(
                Builders<E>.Filter.Eq(t => t.TaleId, taleId), 
                Builders<E>.Filter.Eq(t => t.TaleVersionId, taleVersionId)
                );
            await collection.DeleteManyAsync(filter, token);
        }
    }
}
