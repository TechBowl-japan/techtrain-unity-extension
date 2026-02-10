#nullable enable

namespace TechtrainExtension.Api.Models.v3
{
    public class UsersMeResponse
    {
        public int id { get; set; }
        public string? nickname { get; set; }
        public bool is_agreed_current_term { get; set; }
    }
}
