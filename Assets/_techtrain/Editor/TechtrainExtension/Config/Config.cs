#nullable enable

namespace TechtrainExtension.Config
{
    public class Config
    {
        public ConfigAuthentication auth { get; set; }
        public string? apiEndpoint { get; set; }
        public string? apiEndpointV3 { get; set; }

        public Config()
        {
            auth = new ConfigAuthentication();
        }
    }

    public class ConfigAuthentication
    {
        public string? apiToken { get; set; }
        public string? apiRefreshToken { get; set; }
        public string? apiAuthCookieName { get; set; }
    }
}
