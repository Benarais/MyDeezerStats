using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Repositories;
using MyDeezerStats.Infrastructure.Settings;


namespace MyDeezerStats.Infrastructure.Mongo
{
    public class ListeningRepository : IListeningRepository
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<ListeningRepository> _logger;

        public ListeningRepository(IConfiguration config, ILogger<ListeningRepository> logger)
        {
            var settings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            if (settings is null)
            {
                throw new ArgumentNullException("MongoDbSettings", "MongoDbSettings configuration section is missing.");
            }

            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
            _logger = logger;
        }

        public async Task<List<BsonDocument>> AggregateRawAsync(string groupByField, DateTime? from, DateTime? to, int limit)
        {
            var collection = _database.GetCollection<BsonDocument>("listening");

            var pipeline = new List<BsonDocument>();

            // Filtrage par dates si spécifié
            if (from.HasValue || to.HasValue)
            {
                var dateFilter = new BsonDocument();

                if (from.HasValue)
                {
                    dateFilter.Add("$gte", new BsonDateTime(from.Value));
                    _logger.LogInformation("Filtre date from: {From}", from.Value);
                }

                if (to.HasValue)
                {
                    dateFilter.Add("$lte", new BsonDateTime(to.Value));
                    _logger.LogInformation("Filtre date to: {To}", to.Value);
                }

                pipeline.Add(new BsonDocument("$match",
                    new BsonDocument("Date", dateFilter)));
            }

            // Groupement dynamique
            pipeline.Add(BuildGroupStage(groupByField));

            // Tri et limite
            pipeline.Add(new BsonDocument("$sort", new BsonDocument("count", -1)));
            pipeline.Add(new BsonDocument("$limit", limit));

            var result = await collection.AggregateAsync<BsonDocument>(pipeline);
            return await result.ToListAsync();
        }

        private BsonDocument BuildGroupStage(string groupByField)
        {
            var groupDoc = new BsonDocument
            {
                { "_id", $"${groupByField}" },
                { "count", new BsonDocument("$sum", 1) }
            };

            // Ajout des champs supplémentaires selon le type de groupement
            switch (groupByField.ToLower())
            {
                case "album":
                    groupDoc.Add("artist", new BsonDocument("$first", "$Artist"));
                    break;

                case "track":
                    groupDoc.Add("artist", new BsonDocument("$first", "$Artist"));
                    groupDoc.Add("album", new BsonDocument("$first", "$Album"));
                    break;
            }

            return new BsonDocument("$group", groupDoc);
        }

        public async Task<List<ListeningEntry>> GetLatestListeningsAsync(int limit)
        {
            var collection = _database.GetCollection<BsonDocument>("listening");

            var result = await collection
                .Find(new BsonDocument())
                .Sort(Builders<BsonDocument>.Sort.Descending("Date"))
                .Limit(limit)
                .ToListAsync();

            return result.Select(doc => new ListeningEntry
            {
                Track = doc.GetValue("Track", "").AsString,
                Artist = doc.GetValue("Artist", "").AsString,
                Album = doc.GetValue("Album", "").AsString,
                Date = doc.GetValue("Date", DateTime.MinValue).ToUniversalTime()
            }).ToList();
        }

        public async Task InsertListeningsAsync(List<ListeningEntry> listenings)
        {
            var collection = _database.GetCollection<ListeningEntry>("listening");

            //var tasks = new List<Task>();
            foreach (var listening in listenings)
            {
                var filter = Builders<ListeningEntry>.Filter.And(
                    Builders<ListeningEntry>.Filter.Eq(x => x.Track, listening.Track),
                    Builders<ListeningEntry>.Filter.Eq(x => x.Artist, listening.Artist),
                    Builders<ListeningEntry>.Filter.Eq(x => x.Album, listening.Album),
                    Builders<ListeningEntry>.Filter.Eq(x => x.Date, listening.Date)
                );

                var exists = await collection.Find(filter).AnyAsync();
                if (!exists)
                {
                    await collection.InsertOneAsync(listening);
                }
            }
        }
    }
}
