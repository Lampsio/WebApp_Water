using LevelWater.Models;
using LevelWater.Services;
using Microsoft.AspNetCore.Mvc;

namespace LevelWater.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StationInfoController : ControllerBase
    {
        private readonly MongoDBServices _mongoDBService;

        public StationInfoController(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestMeasurements()
        {
            try
            {
                // Pobierz wszystkie rzeki
                var rivers = await _mongoDBService.GetAllRiversAsync();

                var latestMeasurements = new List<MeasurementWithDetails>();
                foreach (var river in rivers)
                {
                    foreach (var station in river.Stations)
                    {
                        var latestMeasurement = station.Measurements.OrderByDescending(m => m.DateTime).FirstOrDefault();
                        if (latestMeasurement != null)
                        {
                            latestMeasurements.Add(new MeasurementWithDetails
                            {
                                RiverName = river.Name,
                                City = station.City,
                                WarningLevel = station.WarningLevel,
                                AlarmLevel = station.AlarmLevel,
                                Latitude = station.Latitude,
                                Longitude = station.Longitude,
                                MeasurementDateTime = latestMeasurement.DateTime,  // Send only DateTime
                                MeasurementWaterLevel = latestMeasurement.WaterLevel , // Send only WaterLevel
                                Id = station.Id
                            });
                        }
                    }
                }

                // Zwróć dane jako JSON
                return Ok(latestMeasurements);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting latest measurements: {ex.Message}");
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class RiversController : ControllerBase
    {
        private readonly MongoDBServices _mongoDBService;

        public RiversController(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadRivers([FromBody] List<River> rivers)
        {
            await _mongoDBService.InsertRiversAsync(rivers);
            return Ok();
        }

    }

    [Route("api/[controller]")]
    [ApiController]
    public class RiverController : ControllerBase
    {
        private readonly MongoDBServices _mongoDBService;

        public RiverController(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRivers()
        {
            var rivers = await _mongoDBService.GetAllRiversAsync();
            return Ok(rivers);
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class StationsController : ControllerBase
    {
        private readonly MongoDBServices _mongoDBService;

        public StationsController(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpPost("{riverName}")]
        public async Task<IActionResult> AddStationToRiver(string riverName, [FromBody] Station station)
        {
            // Pobierz rzekę o nazwie riverName (np. "San")
            var river = await _mongoDBService.GetRiverByNameAsync(riverName);

            // Dodaj stację do listy stacji w danej rzece
            river.Stations.Add(station);

            // Zaktualizuj rzekę w kolekcji MongoDB
            await _mongoDBService.UpdateRiverAsync(river);

            return Ok();
        }

    }

    [Route("api/[controller]")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        private readonly MongoDBServices _mongoDBService;

        public MeasurementsController(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpPost("{riverName}/{city}")]
        public async Task<IActionResult> AddMeasurementToStation(string riverName, string city, [FromBody] Measurement measurement)
        {
            try
            {
                // Dodaj pomiar do stacji w danej rzece na podstawie nazwy rzeki i miasta
                await _mongoDBService.AddMeasurementToRiverAsync(riverName, city, measurement);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding measurement: {ex.Message}");
            }
        }
    }

    public class MeasurementWithDetails
    {
        public int Id { get; set; }
        public string RiverName { get; set; } = null!;
        public string City { get; set; } = null!;
        public double WarningLevel { get; set; }
        public double AlarmLevel { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Measurement Measurement { get; set; } = null!;
        public DateTime MeasurementDateTime { get; set; }  // Added for DateTime
        public double MeasurementWaterLevel { get; set; }  // Added for WaterLevel   

    }

    [Route("api/[controller]")]
    [ApiController]
    public class MeasurementsDetailsController : ControllerBase
    {
        private readonly MongoDBServices _mongoDBService;

        public MeasurementsDetailsController(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestMeasurements()
        {
            try
            {
                // Pobierz wszystkie rzeki
                var rivers = await _mongoDBService.GetAllRiversAsync();

                var latestMeasurements = new List<MeasurementWithDetails>();
                foreach (var river in rivers)
                {
                    foreach (var station in river.Stations)
                    {
                        var latestMeasurement = station.Measurements.OrderByDescending(m => m.DateTime).FirstOrDefault();
                        if (latestMeasurement != null)
                        {
                            latestMeasurements.Add(new MeasurementWithDetails
                            {
                                RiverName = river.Name,
                                City = station.City,
                                WarningLevel = station.WarningLevel,
                                AlarmLevel = station.AlarmLevel,

                            });
                        }
                    }
                }

                // Zwróć dane jako JSON
                return Ok(latestMeasurements);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting latest measurements: {ex.Message}");
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class WaterHistoryController : ControllerBase
    {
        private readonly MongoDBServices _mongoDBService;

        public WaterHistoryController(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet("{riverName}/{city}")]
        public async Task<IActionResult> GetWaterHistory(string riverName, string city)
        {
            try
            {
                // Pobierz rzekę o określonej nazwie
                var river = await _mongoDBService.GetRiverByNameAsync(riverName);

                // Znajdź stację w mieście
                var station = river.Stations.FirstOrDefault(s => s.City == city);
                if (station == null)
                {
                    throw new Exception($"Station in city '{city}' not found.");
                }

                // Pobierz historię pomiarów wody z danej stacji
                var waterHistory = station.Measurements.OrderBy(m => m.DateTime).ToList();
                return Ok(waterHistory);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting water history: {ex.Message}");
            }
        }
    }


}
