using ExcelDataReader;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Repositories;

namespace MyDeezerStats.Application.ExcelServices
{
    public class ExcelService : IExcelService
    {
        private readonly IListeningRepository _listeningRepository;

        public ExcelService(IListeningRepository listeningRepository)
        {
            _listeningRepository = listeningRepository;
        }

        public async Task ProcessExcelFileAsync(Stream fileStream)
        {
            using var reader = ExcelReaderFactory.CreateReader(fileStream);

            // Lire les données 
            var dataSet = reader.AsDataSet();
            var sheet = dataSet.Tables[0]; 

            var listenings = new List<ListeningEntry>();

            // Parcourir les lignes de la feuille
            foreach (System.Data.DataRow row in sheet.Rows)
            {
                var title = row[0]?.ToString(); // Song Title
                var artist = row[1]?.ToString(); // Artist
                var album = row[3]?.ToString(); // Album Title
                var date = DateTime.TryParse(row[8]?.ToString(), out var parsedDate) ? parsedDate : DateTime.MinValue; // Date

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(album) || date == DateTime.MinValue)
                    continue;

                var listeningEntry = new ListeningEntry
                {
                    Track = title,
                    Artist = artist,
                    Album = album,
                    Date = date
                };

                listenings.Add(listeningEntry);
            }

            await _listeningRepository.InsertListeningsAsync(listenings);
            
        }
    }
}
