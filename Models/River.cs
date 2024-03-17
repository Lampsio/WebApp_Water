using System.Diagnostics.Metrics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using static System.Collections.Specialized.BitVector32;

namespace LevelWater.Models
{
    [BsonIgnoreExtraElements]
    public class River
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("stadions")]
        public List<Station> Stations { get; set; } = null!;
    }

    public class Station
    {
        [BsonElement("_id")]
        public int Id { get; set; } 

        [BsonElement("city")]
        public string City { get; set; } = null!;

        [BsonElement("latitude")]
        public double Latitude { get; set; } 

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("warninglevel")]
        public double WarningLevel { get; set; }

        [BsonElement("alarmlevel")]
        public double AlarmLevel { get; set; }

        [BsonElement("measurement")]
        public List<Measurement> Measurements { get; set; } = null!;
    }
    public class Measurement
    {
        [BsonElement("datetime")]
        public DateTime DateTime { get; set; }

        [BsonElement("waterlevel")]
        public double WaterLevel { get; set; }
    }
}
