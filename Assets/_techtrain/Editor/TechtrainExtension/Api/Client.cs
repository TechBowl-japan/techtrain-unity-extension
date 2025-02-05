#nullable enable

using System.Net.Http;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.VersionControl;
using UnityEngine;

namespace TechtrainExtension.Api
{
    public class Client
    {
        static HttpClient client;
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
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "TechTrainExtension");
        }

        private async Task<T?> CreateGetRequest<T>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (token != null)
            {
                request.Headers.Add("Authorization", $"Bearer {token}");

            }
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        private async Task<U?> CreatePostRequest<T, U>(string url, T payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            if (token != null)
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<U>(responseJson);
        }
    }
}
