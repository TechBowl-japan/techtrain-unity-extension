#nullable enable

using System.Net.Http;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.VersionControl;
using UnityEngine;
using Newtonsoft.Json;
using TechtrainExtension.Config;

namespace TechtrainExtension.Api
{
    public class Client
    {
        static HttpClient client = new HttpClient(new HttpClientHandler { UseCookies = false });
        private string baseUrl = "https://api.techtrain.dev/api/v2";
        private string baseUrlV3 = "https://api.techtrain.dev/api/v3";
        private string? token;
        private string? authCookieName;

        private ConfigManager? configManager;

        public Client(ConfigManager? configManager = null)
        {
            var config = configManager?.Config;
            if (config != null)
            {
                baseUrl = config.apiEndpoint ?? baseUrl;
                baseUrlV3 = config.apiEndpointV3 ?? baseUrlV3;
                token = config.auth?.apiToken ?? null;
                authCookieName = config.auth?.apiAuthCookieName ?? "production_techtrain_user";
            }
            client.DefaultRequestHeaders.Add("User-Agent", "TechTrainExtension");
        }

        private async void SetApiToken(HttpRequestMessage request)
        {
            var isTokenExpired = false; // TODO: Implement token expiration check
            if (isTokenExpired)
            {
                token = await PostRefreshToken();
                configManager?.SetApiToken(token);
            }

            if (token == null)
            {
                return;
            }

            if (IsTokenJWT(token))
            {
                request.Headers.Add("Cookie", $"{authCookieName}={token}");
            }
            else
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
        }

        private bool IsTokenJWT(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return false;
            return true;
        }

        private async Task<T?> CreateGetRequest<T>(string url, bool ensureSuccess = false)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetApiToken(request);
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
            SetApiToken(request);
            var response = await client.SendAsync(request);
            if (ensureSuccess) response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<U>(responseJson);
        }

        public async Task<Models.v3.Response<Models.v3.LoginResponse>?> PostLogin(string email, string password)
        {
            var payload = new Models.v3.LoginBody
            {
                email = email,
                password = password
            };
            return await CreatePostRequest<Models.v3.LoginBody, Models.v3.Response<Models.v3.LoginResponse>>($"{baseUrlV3}/user/auth/login", payload);
        }

        public async Task<Models.v3.Response<Models.v3.UsersMeResponse>?> PostUsersMe()
        {
            return await CreatePostRequest<object, Models.v3.Response<Models.v3.UsersMeResponse>>($"{baseUrlV3}/user/users/me", new object());
        }

        private async Task<string> PostRefreshToken()
        {
            // TODO: Implement refresh token process
            return "new token";
        }
    }
}
