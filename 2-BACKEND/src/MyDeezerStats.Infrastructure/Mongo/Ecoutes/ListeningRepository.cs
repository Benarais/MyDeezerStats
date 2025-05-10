using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.ListeningInfos;
using MyDeezerStats.Domain.Repositories;
using MyDeezerStats.Infrastructure.Settings;

namespace MyDeezerStats.Infrastructure.Mongo.Ecoutes
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

        public async Task<List<AlbumListening>> GetTopAlbumsWithAsync(DateTime? from = null, DateTime? to = null, int limit = 10)
        {
            // Validation des paramètres
            if (limit <= 0 || limit > 100)
                limit = 10;

            var collection = _database.GetCollection<BsonDocument>("listening");

            // Construction du filtre de base avec dates
            var dateFilter = BuildDateFilter(from, to);

            // Filtre complet incluant la vérification des albums non vides
            var completeFilter = Builders<BsonDocument>.Filter.And(
                dateFilter,
                Builders<BsonDocument>.Filter.Exists("Album"),
                Builders<BsonDocument>.Filter.Ne("Album", BsonNull.Value),
                Builders<BsonDocument>.Filter.Ne("Album", ""),
                Builders<BsonDocument>.Filter.Exists("Track"),
                Builders<BsonDocument>.Filter.Ne("Track", BsonNull.Value),
                Builders<BsonDocument>.Filter.Ne("Track", "")
            );

            var pipeline = new[]
            {
                // Étape 1: Filtrer les documents
                PipelineStageDefinitionBuilder.Match(completeFilter),

                // Étape 2: Projeter uniquement les champs nécessaires pour améliorer les performances
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "Album", 1 },
                        { "Artist", 1 },
                        { "Track", 1 },
                        { "NormalizedAlbum", new BsonDocument("$trim", new BsonDocument("input", "$Album")) },
                        { "NormalizedTrack", new BsonDocument("$trim", new BsonDocument("input", "$Track")) }
                    }),

                // Étape 3: Normaliser les artistes (premier artiste avant virgule, trimé)
                new BsonDocument("$addFields",
                    new BsonDocument("PrimaryArtist",
                        new BsonDocument("$let",
                            new BsonDocument
                            {
                                { "vars",
                                    new BsonDocument("artists",
                                        new BsonDocument("$split", new BsonArray { "$Artist", "," }))
                                },
                                { "in",
                                    new BsonDocument("$trim",
                                        new BsonDocument("input",
                                            new BsonDocument("$cond",
                                                new BsonArray
                                                {
                                                    new BsonDocument("$gt", new BsonArray { new BsonDocument("$size", "$$artists"), 0 }),
                                                    new BsonDocument("$arrayElemAt", new BsonArray { "$$artists", 0 }),
                                                    ""
                                                })))
                                }
                            }))),

                // Étape 4: Grouper par Album + Artiste principal + Track
                new BsonDocument("$group",
                    new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "Album", "$NormalizedAlbum" },
                                { "Artist", "$PrimaryArtist" },
                                { "Track", "$NormalizedTrack" }
                            }
                        },
                        { "Count", new BsonDocument("$sum", 1) }
                    }),

                // Étape 5: Regrouper par Album + Artiste principal
                new BsonDocument("$group",
                    new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "Album", "$_id.Album" },
                                { "Artist", "$_id.Artist" }
                            }
                        },
                        { "Tracks", new BsonDocument("$push",
                            new BsonDocument
                            {
                                { "Track", "$_id.Track" },
                                { "Count", "$Count" }
                            })},
                        { "TotalCount", new BsonDocument("$sum", "$Count") }
                    }),

                // Étape 6: Trier par nombre d'écoutes décroissant
                new BsonDocument("$sort", new BsonDocument("TotalCount", -1)),

                // Étape 7: Limiter aux résultats demandés
                new BsonDocument("$limit", limit),

                // Étape 8: Projeter le résultat final
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "Title", "$_id.Album" },
                        { "Artist", "$_id.Artist" },
                        { "StreamCount", "$TotalCount" },
                        { "StreamCountByTrack", "$Tracks" },
                        { "_id", 0 }
                    })
            };

            var results = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return results.Select(doc => new AlbumListening
            {
                Title = doc["Title"].AsString,
                Artist = doc["Artist"].AsString,
                StreamCount = doc["StreamCount"].AsInt32,
                StreamCountByTrack = doc["StreamCountByTrack"].AsBsonArray
                    .Select(t => new KeyValuePair<string, int>(
                        t["Track"].AsString,
                        t["Count"].AsInt32))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            }).ToList();
        }

        public async Task<List<ArtistListening>> GetTopArtistsWithAsync(DateTime? from = null, DateTime? to = null, int limit = 10)
        {
            // Validation des paramètres
            if (limit <= 0 || limit > 100)
                limit = 10;

            var collection = _database.GetCollection<BsonDocument>("listening");

            // Construction du filtre de base avec dates
            var dateFilter = BuildDateFilter(from, to);

            // Filtre complet incluant la vérification des artistes et morceaux non vides
            var completeFilter = Builders<BsonDocument>.Filter.And(
                dateFilter,
                Builders<BsonDocument>.Filter.Exists("Artist"),
                Builders<BsonDocument>.Filter.Ne("Artist", BsonNull.Value),
                Builders<BsonDocument>.Filter.Ne("Artist", ""),
                Builders<BsonDocument>.Filter.Exists("Track"),
                Builders<BsonDocument>.Filter.Ne("Track", BsonNull.Value),
                Builders<BsonDocument>.Filter.Ne("Track", "")
            );

            var pipeline = new[]
            {
                // Étape 1: Filtrer les documents
                PipelineStageDefinitionBuilder.Match(completeFilter),

                // Étape 2: Projeter uniquement les champs nécessaires
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "Artist", 1 },
                        { "Track", 1 },
                        { "PrimaryArtist", new BsonDocument("$trim",
                            new BsonDocument("input",
                                new BsonDocument("$arrayElemAt",
                                    new BsonArray { new BsonDocument("$split", new BsonArray { "$Artist", "," }), 0 }))) },
                        { "NormalizedTrack", new BsonDocument("$trim", new BsonDocument("input", "$Track")) }
                    }),

                // Étape 3: Grouper par Artiste + Track
                new BsonDocument("$group",
                    new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "Artist", "$PrimaryArtist" },
                                { "Track", "$NormalizedTrack" }
                            }
                        },
                        { "Count", new BsonDocument("$sum", 1) }
                    }),

                // Étape 4: Regrouper par Artiste
                new BsonDocument("$group",
                    new BsonDocument
                    {
                        { "_id", "$_id.Artist" },
                        { "Tracks", new BsonDocument("$push",
                            new BsonDocument
                            {
                                { "Track", "$_id.Track" },
                                { "Count", "$Count" }
                            })},
                        { "TotalCount", new BsonDocument("$sum", "$Count") }
                    }),

                // Étape 5: Trier par nombre d'écoutes décroissant
                new BsonDocument("$sort", new BsonDocument("TotalCount", -1)),

                // Étape 6: Limiter aux résultats demandés
                new BsonDocument("$limit", limit),

                // Étape 7: Projeter le résultat final
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "Name", "$_id" },
                        { "StreamCount", "$TotalCount" },
                        { "StreamCountByTrack", "$Tracks" },
                        { "_id", 0 }
                    })
            };

            var results = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return results.Select(doc => new ArtistListening
            {
                Name = doc["Name"].AsString,
                StreamCount = doc["StreamCount"].AsInt32,
                StreamCountByTrack = doc["StreamCountByTrack"].AsBsonArray
                    .Select(t => new KeyValuePair<string, int>(
                        t["Track"].AsString,
                        t["Count"].AsInt32))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            }).ToList();
        }

        public async Task<List<TrackListening>> GetTopTracksWithAsync(DateTime? from = null, DateTime? to = null, int limit = 10)
        {
            // Validation des paramètres
            if (limit <= 0 || limit > 100)
                limit = 10;

            var collection = _database.GetCollection<BsonDocument>("listening");

            // Construction du filtre de base avec dates
            var dateFilter = BuildDateFilter(from, to);

            // Filtre complet incluant la vérification des champs obligatoires
            var completeFilter = Builders<BsonDocument>.Filter.And(
                dateFilter,
                Builders<BsonDocument>.Filter.Exists("Track"),
                Builders<BsonDocument>.Filter.Ne("Track", BsonNull.Value),
                Builders<BsonDocument>.Filter.Ne("Track", ""),
                Builders<BsonDocument>.Filter.Exists("Artist"),
                Builders<BsonDocument>.Filter.Ne("Artist", BsonNull.Value),
                Builders<BsonDocument>.Filter.Ne("Artist", "")
            );

            var pipeline = new[]
            {
                // Étape 1: Filtrer les documents
                PipelineStageDefinitionBuilder.Match(completeFilter),

                // Étape 2: Normaliser les données et projeter les champs nécessaires
                new BsonDocument("$addFields",
                    new BsonDocument
                    {
                        { "NormalizedTrack", new BsonDocument("$trim", new BsonDocument("input", "$Track")) },
                        { "PrimaryArtist", new BsonDocument("$trim",
                            new BsonDocument("input",
                                new BsonDocument("$arrayElemAt",
                                    new BsonArray { new BsonDocument("$split", new BsonArray { "$Artist", "," }), 0 }))) },
                        { "NormalizedAlbum", new BsonDocument("$ifNull",
                            new BsonArray { "$Album", "Unknown Album" }) }
                    }),

                // Étape 3: Grouper par Morceau + Artiste + Album
                new BsonDocument("$group",
                    new BsonDocument
                    {
                        { "_id", new BsonDocument
                            {
                                { "Track", "$NormalizedTrack" },
                                { "Artist", "$PrimaryArtist" },
                                { "Album", "$NormalizedAlbum" }
                            }
                        },
                        { "StreamCount", new BsonDocument("$sum", 1) }
                    }),

                // Étape 4: Trier par nombre d'écoutes décroissant
                new BsonDocument("$sort", new BsonDocument("StreamCount", -1)),

                // Étape 5: Limiter aux résultats demandés
                new BsonDocument("$limit", limit),

                // Étape 6: Projeter le résultat final
                new BsonDocument("$project",
                    new BsonDocument
                    {
                        { "Name", "$_id.Track" },
                        { "Artist", "$_id.Artist" },
                        { "Album", "$_id.Album" },
                        { "StreamCount", 1 },
                        { "_id", 0 }
                    })
            };

            var results = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return results.Select(doc => new TrackListening
            {
                Name = doc["Name"].AsString,
                Artist = doc["Artist"].AsString,
                Album = doc["Album"].AsString,
                StreamCount = doc["StreamCount"].AsInt32
            }).ToList();
        }

        private FilterDefinition<BsonDocument> BuildDateFilter(DateTime? from, DateTime? to)
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Empty;

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