using ExcelDataReader;
using Microsoft.Extensions.Logging;
using MyDeezerStats.Application.Interfaces;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MyDeezerStats.Application.ExcelServices
{
    public class ExcelService : IExcelService
    {
        private readonly IListeningRepository _listeningRepository;
        private readonly ILogger<ExcelService> _logger;

        public ExcelService(
            IListeningRepository listeningRepository,
            ILogger<ExcelService> logger)
        {
            _listeningRepository = listeningRepository ?? throw new ArgumentNullException(nameof(listeningRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessExcelFileAsync(Stream fileStream, int batchSize = 1000)
        {
            var stopwatch = Stopwatch.StartNew();
            int totalProcessed = 0;

            try
            {
                if (fileStream == null || fileStream.Length == 0)
                {
                    throw new ArgumentException("Le flux de fichier fourni est vide ou non valide");
                }

                _logger.LogInformation("Début du traitement du fichier Excel");

                using var reader = ExcelReaderFactory.CreateReader(fileStream);

                var configuration = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true 
                    }
                };

                var dataSet = reader.AsDataSet(configuration);
                var sheet = dataSet.Tables[0];

                var listeningsBatch = new List<ListeningEntry>(batchSize);
                var totalRows = sheet.Rows.Count;
                var skippedRows = 0;

                for (int i = 1; i < totalRows; i++) // Commencer à 1 pour sauter l'en-tête
                {
                    var row = sheet.Rows[i];

                    if (!TryParseRow(row, out var listeningEntry))
                    {
                        skippedRows++;
                        continue;
                    }

                    listeningsBatch.Add(listeningEntry);

                    if (listeningsBatch.Count >= batchSize)
                    {
                        await _listeningRepository.InsertListeningsAsync(listeningsBatch);
                        //totalProcessed += inserted;
                        listeningsBatch.Clear();
                    }
                }

                // Traiter le dernier batch s'il reste des éléments
                if (listeningsBatch.Count > 0)
                {
                    await _listeningRepository.InsertListeningsAsync(listeningsBatch);
                    //totalProcessed += inserted;
                }

                _logger.LogInformation(
                    "Traitement terminé en {ElapsedMs} ms. {Processed} écoutes traitées, {Skipped} lignes ignorées.",
                    stopwatch.ElapsedMilliseconds,
                    totalProcessed,
                    skippedRows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du fichier Excel");
                throw new ApplicationException("Une erreur est survenue lors du traitement du fichier Excel", ex);
            }
        }

        public async Task<int> ProcessLargeExcelFileStreamedAsync(Stream fileStream, int batchSize = 1000)
        {
            var stopwatch = Stopwatch.StartNew();
            int totalProcessed = 0;
            int rowCount = 0;
            int skippedRows = 0;

            try
            {
                _logger.LogInformation("Début du traitement en mode streamé");

                using var reader = ExcelReaderFactory.CreateReader(fileStream);
                var listeningsBatch = new List<ListeningEntry>(batchSize);

                do
                {
                    while (reader.Read())
                    {
                        // Sauter la première ligne si c'est un en-tête
                        if (rowCount++ == 0 && IsHeaderRow(reader))
                            continue;

                        if (!TryParseReaderRow(reader, out var listeningEntry))
                        {
                            skippedRows++;
                            continue;
                        }

                        listeningsBatch.Add(listeningEntry);

                        if (listeningsBatch.Count >= batchSize)
                        {
                            await _listeningRepository.InsertListeningsAsync(listeningsBatch);
                            totalProcessed += 1;
                            listeningsBatch.Clear();
                        }
                    }
                } while (reader.NextResult()); // Pour gérer plusieurs feuilles

                // Dernier batch
                if (listeningsBatch.Count > 0)
                {
                    await _listeningRepository.InsertListeningsAsync(listeningsBatch);
                    //totalProcessed += inserted;
                }

                _logger.LogInformation(
                    "Traitement streamé terminé en {ElapsedMs} ms. {Processed} écoutes traitées, {Skipped} lignes ignorées.",
                    stopwatch.ElapsedMilliseconds,
                    totalProcessed,
                    skippedRows);

                return totalProcessed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement streamé du fichier Excel");
                throw;
            }
        }

        private bool IsHeaderRow(IExcelDataReader reader)
        {
            // Vérifie si la première cellule contient une valeur qui ressemble à un en-tête
            try
            {
                var firstCellValue = reader.GetString(0);
                return !string.IsNullOrEmpty(firstCellValue) &&
                       !DateTime.TryParse(firstCellValue, out _);
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseRow(System.Data.DataRow row, out ListeningEntry listeningEntry)
        {
            listeningEntry = new ListeningEntry();

            try
            {
                var title = row[0]?.ToString()?.Trim();
                var artist = row[1]?.ToString()?.Trim();
                var album = row[3]?.ToString()?.Trim();

                if (!DateTime.TryParse(row[8]?.ToString(), out var date))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(album))
                {
                    return false;
                }

                listeningEntry = new ListeningEntry
                {
                    Track = title,
                    Artist = artist,
                    Album = album,
                    Date = date
                };

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseReaderRow(IExcelDataReader reader, out ListeningEntry listeningEntry)
        {
            listeningEntry = new ListeningEntry();

            try
            {
                var title = reader[0]?.ToString()?.Trim();
                var artist = reader[1]?.ToString()?.Trim();
                var album = reader[3]?.ToString()?.Trim();

                if (!DateTime.TryParse(reader[8]?.ToString(), out var date))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(album))
                {
                    return false;
                }

                listeningEntry = new ListeningEntry
                {
                    Track = title,
                    Artist = artist,
                    Album = album,
                    Date = date
                };

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}