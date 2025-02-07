#nullable enable

using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TechtrainExtension.Api
{
    public class Client
    {
        static HttpClient client = new HttpClient();
        private string baseUrl = "https://api.techtrain.dev/api/v2";
        private string baseUrlV3 = "https://api.techtrain.dev/api/v3";
        private string? token;

        public Client(Config.Config? config = null)
        {
            if (config != null)
            {
                baseUrl = config.apiEndpoint ?? baseUrl;
                baseUrlV3 = config.apiEndpointV3 ?? baseUrlV3;
                token = config.auth?.apiToken ?? null;
            }
            client.DefaultRequestHeaders.Add("User-Agent", "TechTrainExtension");
        }

        private async Task<T?> CreateGetRequest<T>(string url, bool ensureSuccess = false)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (token != null)
            {
                request.Headers.Add("Authorization", $"Bearer {token}");

            }
            var response = await client.SendAsync(request);
            if (ensureSuccess) response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        private async Task<U?> CreatePostRequest<T, U>(string url, T payload, bool ensureSuccess = false)
        {
            var json = JsonConvert.SerializeObject(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            if (token != null)
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
            var response = await client.SendAsync(request);
            if (ensureSuccess) response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<U>(responseJson);
        }

        public async Task<Models.v3.Response<Models.v3.Railway>?> GetRailway(int railwayId)
        {
            return await CreateGetRequest<Models.v3.Response<Models.v3.Railway>>($"{baseUrlV3}/techtrain/user/railways/{railwayId}");
        }
    }
}
