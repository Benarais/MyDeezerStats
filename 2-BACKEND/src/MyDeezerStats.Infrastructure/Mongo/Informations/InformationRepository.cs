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


        public async Task<AlbumInfos?> GetAlbumInfosAsync(string artist, string album)
        {
            var collection = _database.GetCollection<BsonDocument>("albumInfo"); 

            var filter = Builders<BsonDocument>.Filter.Eq("Artist", artist) &
                         Builders<BsonDocument>.Filter.Eq("Album", album);

            var projection = Builders<BsonDocument>.Projection
                .Include("Track")
                .Include("Duration")
                .Include("AlbumUrl");

            var tracks = await collection.Find(filter).Project(projection).ToListAsync();

            if (tracks == null || !tracks.Any())
                return null;

            int totalDuration = 0;
            var uniqueTracks = new HashSet<string>();
            string? albumUrl = null;

            foreach (var track in tracks)
            {
                if (track.Contains("Track"))
                    uniqueTracks.Add(track["Track"].AsString);

                if (track.Contains("Duration") && track["Duration"].IsInt32)
                    totalDuration += track["Duration"].AsInt32;

                if (albumUrl == null && track.Contains("AlbumUrl"))
                    albumUrl = track["AlbumUrl"].AsString;
            }

            return new AlbumInfos
            {
                Title = album,
                Artist = artist,
                Duration = totalDuration,
                TrackNumber = uniqueTracks.Count,
                AlbumUrl = albumUrl ?? string.Empty
            };
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
                Title = doc.GetValue("Title", "").AsString,
                Album = doc.GetValue("Album", "").AsString,
                Artist = doc.GetValue("Artist", "").AsString,
                TrackUrl = doc.GetValue("TrackUrl", "").AsString,
                Duration = doc.Contains("Duration") && doc["Duration"].IsInt32
                    ? doc["Duration"].AsInt32
                    : 0
            };
        }

        public async Task InsertAlbumInfosAsync(AlbumInfos album)
        {
            if (album == null) throw new ArgumentNullException(nameof(album));

            var collection = _database.GetCollection<AlbumInfos>("albumInfo");

            var filter = Builders<AlbumInfos>.Filter.And(
                Builders<AlbumInfos>.Filter.Eq(a => a.Title, album.Title),
                Builders<AlbumInfos>.Filter.Eq(a => a.Artist, album.Artist)
            );

            var options = new ReplaceOptions { IsUpsert = true };

            await collection.ReplaceOneAsync(filter, album, options);
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

            var collection = _database.GetCollection<TrackInfos>("trackInfo");

            var filter = Builders<TrackInfos>.Filter.And(
                Builders<TrackInfos>.Filter.Eq(t => t.Title, track.Title),
                Builders<TrackInfos>.Filter.Eq(t => t.Album, track.Album),
                Builders<TrackInfos>.Filter.Eq(t => t.Artist, track.Artist)
            );

            var options = new ReplaceOptions { IsUpsert = true };

            await collection.ReplaceOneAsync(filter, track, options);
        }
    }
}
