#nullable enable

using System.Collections.Generic;

namespace TechtrainExtension.Manifests.Models
{
    public sealed class Station
    {
        public string name { get; set; }
        public int id { get; set; }
        public List<StationPrepare>? prepare { get; set; }
        public List<StationTest> tests { get; set; }
    }

    public sealed class StationPrepare
    {
        public string command { get; set; }
        public bool background { get; set; }
    }

    public sealed class StationTest
    {
        public string type { get; set; }
        public string? title { get; set; }
        public string command { get; set; }
        public List<string> args { get; set; }
        public bool shell { get; set; }
    }
}
