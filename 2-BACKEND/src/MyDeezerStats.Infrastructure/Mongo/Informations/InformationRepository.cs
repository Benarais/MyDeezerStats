using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Entities.DeezerInfos;
using MyDeezerStats.Domain.Repositories;
using MyDeezerStats.Infrastructure.Mongo.Ecoutes;
using MyDeezerStats.Infrastructure.Settings;

namespace MyDeezerStats.Infrastructure.Mongo.Informations
{
    public class InformationRepository : IInformationRepository
    {

        private readonly IMongoDatabase _database;
        private readonly ILogger<InformationRepository> _logger;

        public InformationRepository(IConfiguration config, ILogger<InformationRepository> logger)
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

        public async Task<ArtistInfos?> GetArtistInfosAsync(string artist)
        {
            var collection = _database.GetCollection<BsonDocument>("artistInfo");

            var filter = Builders<BsonDocument>.Filter.Eq("Artist", artist);

            var projection = Builders<BsonDocument>.Projection
                .Include("Name")
                .Include("ArtistUrl");

            var doc = await collection.Find(filter).Project(projection).FirstOrDefaultAsync();

            if (doc == null) return null;

            return new ArtistInfos
            {
                Name = doc.GetValue("Name", "").AsString,
                ArtistUrl = doc.GetValue("ArtistUrl", "").AsString
            };
        }

        public async Task<TrackInfos?> GetTrackInfosAsync(string track, string artist)
        {
            var collection = _database.GetCollection<BsonDocument>("trackInfo");


            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("Artist", artist),
                Builders<BsonDocument>.Filter.Eq("Title", track)
            );
            var projection = Builders<BsonDocument>.Projection
                .Include("Title")
                .Include("Artist")
                .Include("Album")
                .Include("Duration")
                .Include("TrackUrl");
            
            var doc = await collection.Find(filter).Project(projection).FirstOrDefaultAsync();

            if (doc == null)
            {
                _logger.LogInformation("No track found for Artist: {Artist} and Track: {Track}", artist, track);
                return null;
            }

            _logger.LogInformation("Document retrieved: {Doc}", doc.ToJson());

            return new TrackInfos
            {
                Title = doc.TryGetValue("Title", out var titleVal) && titleVal.IsString ? titleVal.AsString : "",
                Album = doc.TryGetValue("Album", out var albumVal) && albumVal.IsString ? albumVal.AsString : "",
                Artist = doc.TryGetValue("Artist", out var artistVal) && artistVal.IsString ? artistVal.AsString : "",
                TrackUrl = doc.TryGetValue("TrackUrl", out var urlVal) && urlVal.IsString ? urlVal.AsString : "",
                Duration = doc.TryGetValue("Duration", out var durationVal) && durationVal.IsInt32 ? durationVal.AsInt32 : 0
            };
        }

        public async Task InsertArtistInfosAsync(ArtistInfos artistInfos)
        {
            if (artistInfos == null) throw new ArgumentNullException(nameof(artistInfos));

            var collection = _database.GetCollection<ArtistInfos>("artistInfo");

            var filter = Builders<ArtistInfos>.Filter.And(
                Builders<ArtistInfos>.Filter.Eq(t => t.Name, artistInfos.Name)
            );

            var options = new ReplaceOptions { IsUpsert = true };

            await collection.ReplaceOneAsync(filter,artistInfos, options);

        }

        public async Task InsertTrackInfosAsync(TrackInfos track)
        {
            if (track == null) throw new ArgumentNullException(nameof(track));

            // Normalisation des données
            track.Title = string.IsNullOrWhiteSpace(track.Title) ? "Unknown Title" : track.Title.Trim();
            track.Artist = string.IsNullOrWhiteSpace(track.Artist) ? "Unknown Artist" : track.Artist.Trim();

            var collection = _database.GetCollection<TrackInfos>("trackInfo");

            // Filtre plus flexible pour l'upsert
            var filter = Builders<TrackInfos>.Filter.And(
                Builders<TrackInfos>.Filter.Eq(t => t.Title, track.Title),
                Builders<TrackInfos>.Filter.Eq(t => t.Artist, track.Artist)
            );

            var options = new ReplaceOptions { IsUpsert = true };

            try
            {
                await collection.ReplaceOneAsync(filter, track, options);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Fallback pour les cas de duplication
                if (ex.WriteError.Message.Contains("Artist_1_Track_1"))
                {
                    // Solution 1: Générer un ID artificiel pour le Track
                    track.Title = $"{track.Title}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
                    await collection.ReplaceOneAsync(filter, track, options);
                }
                else
                {
                    throw; // Relancer les autres types d'erreurs
                }
            }
        }

    }
}
