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

        public async Task<List<AlbumStatistic>> GetTopAlbumsWithTracksAsync(DateTime? from = null, DateTime? to = null, int limit = 20)
        {
            var collection = _database.GetCollection<BsonDocument>("listening");
            var filter = BuildDateFilter(from, to);

            var pipeline = new[]
            {
        PipelineStageDefinitionBuilder.Match(filter),
        new BsonDocument("$group",
            new BsonDocument
            {
                { "_id",
                    new BsonDocument
                    {
                        { "Album", "$Album" },
                        { "Artist", "$Artist" },
                        { "Track", "$Track" }
                    }
                },
                { "Count", new BsonDocument("$sum", 1) }
            }),
        new BsonDocument("$group",
            new BsonDocument
            {
                { "_id",
                    new BsonDocument
                    {
                        { "Album", "$_id.Album" },
                        { "Artist", "$_id.Artist" }
                    }
                },
                { "Listening",
                    new BsonDocument("$push",
                        new BsonDocument
                        {
                            { "Name", "$_id.Track" },
                            { "Count", "$Count" }
                        })
                },
                { "TotalListens", new BsonDocument("$sum", "$Count") }
            }),
        new BsonDocument("$sort", new BsonDocument("TotalListens", -1)),
        new BsonDocument("$limit", limit)
    };

            var bsonResults = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return bsonResults.Select(doc => new AlbumStatistic
            {
                Album = doc["_id"]["Album"].AsString,
                Artist = doc["_id"]["Artist"].AsString,
                AlbumTotalListening = doc["TotalListens"].AsInt32,
                Listening = doc["Listening"].AsBsonArray.Select(t => new ListeningInfo
                {
                    Name = t["Name"].AsString,
                    Count = t["Count"].AsInt32,
                    TotalDuration = 0, // À mettre à jour avec les données réelles
                    TotalListening = t["Count"].AsInt32
                }).ToList()
            }).ToList();
        }


        public async Task<List<ArtistStatistic>> GetTopArtistsWithTracksAsync(DateTime? from = null, DateTime? to = null, int limit = 20)
        {
            var collection = _database.GetCollection<BsonDocument>("listening");
            var filter = BuildDateFilter(from, to);

            var pipeline = new[]
            {
        PipelineStageDefinitionBuilder.Match(filter),
        new BsonDocument("$group",
            new BsonDocument
            {
                { "_id",
                    new BsonDocument
                    {
                        { "Artist", "$Artist" },
                        { "Track", "$Track" }
                    }
                },
                { "Count", new BsonDocument("$sum", 1) }
            }),
        new BsonDocument("$group",
            new BsonDocument
            {
                { "_id", "$_id.Artist" },
                { "Listening",
                    new BsonDocument("$push",
                        new BsonDocument
                        {
                            { "Name", "$_id.Track" },
                            { "Count", "$Count" }
                        })
                },
                { "TotalListens", new BsonDocument("$sum", "$Count") }
            }),
        new BsonDocument("$sort", new BsonDocument("TotalListens", -1)),
        new BsonDocument("$limit", limit)
    };

            var bsonResults = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return bsonResults.Select(doc => new ArtistStatistic
            {
                Artist = doc["_id"].AsString,
                Listening = doc["Listening"].AsBsonArray.Select(t => new ListeningInfo
                {
                    Name = t["Name"].AsString,
                    Count = t["Count"].AsInt32,
                    TotalDuration = 0 // Initialisé à 0
                }).ToList()
            }).ToList();
        }

        public async Task<List<TrackStatistic>> GetTopTracksWithAsync(DateTime? from = null, DateTime? to = null, int limit = 10)
        {
            var collection = _database.GetCollection<BsonDocument>("listening");
            var filter = BuildDateFilter(from, to);

            var pipeline = new[]
            {
        PipelineStageDefinitionBuilder.Match(filter),
        new BsonDocument("$group",
            new BsonDocument
            {
                { "_id",
                    new BsonDocument
                    {
                        { "Track", "$Track" },
                        { "Artist", "$Artist" },
                        { "Album", "$Album" }
                    }
                },
                { "Count", new BsonDocument("$sum", 1) }
            }),
        new BsonDocument("$project",
            new BsonDocument
            {
                { "Track", "$_id.Track" },
                { "Artist", "$_id.Artist" },
                { "Album", "$_id.Album" },
                { "Count", 1 }
            }),
        new BsonDocument("$sort",
            new BsonDocument("Count", -1)),
        new BsonDocument("$limit", limit)
    };

            var bsonResults = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return bsonResults.Select(doc => new TrackStatistic
            {
                Track = doc["Track"].AsString,
                Artist = doc["Artist"].AsString,
                Album = doc["Album"].AsString,
                TrackDuration = 0,
                TrackNumberListening = doc["Count"].AsInt32, 
                TrackTotalListening = 0
            }).ToList();
        }

        private FilterDefinition<BsonDocument> BuildDateFilter(DateTime? from, DateTime? to)
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Empty;

            // Version 1: Si les dates sont stockées comme DateTime dans MongoDB
            if (from.HasValue)
            {
                filter &= builder.Gte("Date", from.Value);
            }
            if (to.HasValue)
            {
                filter &= builder.Lte("Date", to.Value);
            }

            return filter;
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
