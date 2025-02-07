#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TechtrainExtension.Api.Models.v3
{
    public enum RailwayStationConfirmationMethod
    {
        unit_test,
        form,
        meeting
    }

    public enum RailwayStationAccessLevel
    {
        free,
        paid
    }

    public enum UserRailwayStationStatus
    {
        not_challenging,
        challenging,
        completed
    }

    public class RailwayStation
    {
        public int id { get; set; }
        public int order { get; set; }
        public string title { get; set; }
        public string sub_title { get; set; }
        public string description { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RailwayStationConfirmationMethod confirmation_method { get; set; }
        public RailwayStationClearCondition[] railway_station_clear_conditions { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RailwayStationAccessLevel access_level { get; set; }
        public UserRailwayStation? user_railway_station { get; set; }
    }

    public class RailwayStationClearCondition
    {
        public int id { get; set; }
        public string contents { get; set; }
    }

    public class UserRailwayStation
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public UserRailwayStationStatus status { get; set; }
    }
}