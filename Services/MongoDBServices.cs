using LevelWater.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;

namespace LevelWater.Services
{
    public class MongoDBServices
    {
        private readonly IMongoCollection<River> _riverCollection;

        public MongoDBServices(IOptions<MongoDBSettings> mongoDbSettings)
        {
            MongoClient client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _riverCollection = database.GetCollection<River>(mongoDbSettings.Value.CollectionName);
        }

        public async Task InsertRiverAsync(River river)
        {
            await _riverCollection.InsertOneAsync(river);
        }

        public async Task InsertRiversAsync(List<River> rivers)
        {
            await _riverCollection.InsertManyAsync(rivers);
        }

        public async Task<List<River>> GetAllRiversAsync()
        {
            var filter = new BsonDocument(); // pusty filtr, aby pobrać wszystkie rzeki
            var sort = Builders<River>.Sort.Ascending(r => r.Name); // sortowanie według nazwy

            var rivers = await _riverCollection.FindAsync(filter, new FindOptions<River> { Sort = sort });
            return rivers.ToList();
        }

        public async Task<River> GetRiverByNameAsync(string riverName)
        {
            // Pobranie rzeki o określonej nazwie
            var filter = Builders<River>.Filter.Eq(r => r.Name, riverName);
            return await _riverCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateRiverAsync(River river)
        {
            // Aktualizacja rzeki w kolekcji MongoDB
            var filter = Builders<River>.Filter.Eq(r => r.Id, river.Id);
            await _riverCollection.ReplaceOneAsync(filter, river);
        }

        public async Task AddMeasurementToRiverAsync(string riverName, string city, Measurement measurement)
        {
            // Pobierz rzekę o określonej nazwie
            var river = await GetRiverByNameAsync(riverName);

            // Znajdź stację w mieście
            var station = river.Stations.Find(s => s.City == city);
            if (station == null)
            {
                throw new Exception($"Station in city '{city}' not found.");
            }

            // Dodaj pomiar do stacji
            station.Measurements.Add(measurement);

            // Zaktualizuj rzekę w kolekcji MongoDB
            await UpdateRiverAsync(river);
        }
    }

    [Route("[controller]")]
    [ApiController]
    public class WaterHistoryController : ControllerBase
    {
        private readonly MongoDBServices _mongoDBService;

        public WaterHistoryController(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet("{city}")]
        public async Task<IActionResult> GetWaterHistory(string city)
        {
            try
            {
                // Pobierz wszystkie rzeki
                var rivers = await _mongoDBService.GetAllRiversAsync();

                // Znajdź wszystkie stacje w podanym mieście
                var stations = rivers.SelectMany(r => r.Stations.Where(s => s.City == city)).ToList();

                // Jeśli nie ma stacji w tym mieście
                if (!stations.Any())
                {
                    return NotFound($"Brak stacji w mieście '{city}'.");
                }

                // Pobierz historię pomiarów dla wszystkich stacji w mieście
                var waterHistory = stations.SelectMany(s => s.Measurements.OrderBy(m => m.DateTime)).ToList();

                // Zwróć dane jako JSON
                return Ok(waterHistory);
            }
            catch (Exception ex)
            {
                return BadRequest($"Błąd podczas pobierania historii poziomu wody: {ex.Message}");
            }
        }
    }

}
