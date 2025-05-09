#nullable enable

using System.Collections.Generic;

namespace TechtrainExtension.Manifests.Models
{
    public sealed class Railway
    {
        public int railwayId { get; set; }
        public Dictionary<string, string>? stations { get; set; }
    }
}
