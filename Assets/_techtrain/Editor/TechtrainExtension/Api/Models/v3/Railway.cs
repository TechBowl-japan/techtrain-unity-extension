#nullable enable

namespace TechtrainExtension.Api.Models.v3
{
    public class Railway
    {
        public int id { get; set; }
        public string title { get; set; }
        public string sub_title { get; set; }
        public int total_stations_count { get; set; }
        public int clear_stations_count { get; set; }
        public RailwayStation[] railway_stations { get; set; }
    }
}
