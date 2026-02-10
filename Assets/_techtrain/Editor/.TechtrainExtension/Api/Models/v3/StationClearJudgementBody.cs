#nullable enable

namespace TechtrainExtension.Api.Models.v3
{
    public class StationClearJudgementBody
    {
        public int order { get; set; }
        public bool is_clear { get; set; }
        public string? error_content { get; set; }
    }
}
