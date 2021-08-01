using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EveractiveApiDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseUrl = Environment.GetEnvironmentVariable("EVERACTIVE_API_URL");
            string clientId = Environment.GetEnvironmentVariable("EVERACTIVE_CLIENT_ID");
            string clientSecret = Environment.GetEnvironmentVariable("EVERACTIVE_CLIENT_SECRET");

            if (string.IsNullOrEmpty(baseUrl) ||
                string.IsNullOrEmpty(clientId) || 
                string.IsNullOrEmpty(clientSecret)) 
            {
                Console.WriteLine("EVERACTIVE_API_URL, EVERACTIVE_CLIENT_ID and EVERACTIVE_CLIENT_SECRET must be defined");
                Console.ReadLine();
                return;
            }

            var apiService = new ApiService(baseUrl, clientId, clientSecret);
            var traps = apiService.GetSteamTraps().Result;

            Console.WriteLine($"Returned {traps.Count} steam traps");
            Console.ReadLine();
        }
    }

    class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private AuthToken _authToken = new AuthToken();
        private string _clientId;
        private string _clientSecret;

        public ApiService(
            string baseAddress,
            string clientId,
            string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;

            _httpClient.BaseAddress = new Uri(baseAddress);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Boolean> HealthCheck()
        {            
            var response = await _httpClient.GetAsync("v2020-07/health");
            var b = response.IsSuccessStatusCode;
            return response.IsSuccessStatusCode;
        }

        public async Task<List<SteamTrap>> GetSteamTraps()
        {
            var token = await _getAuthToken();
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _httpClient.GetAsync("v2020-07/steamtraps?page=1&pageSize=25");
            var resultJson = await response.Content.ReadAsStringAsync();
            var steamTrapRsp = JsonConvert.DeserializeObject<EveractiveRspWrapper<List<SteamTrap>>>(resultJson);
            return steamTrapRsp.Data;
        }

        private async Task<AuthToken> _getAuthToken()
        {
            if (this._authToken.Expires <= DateTime.UtcNow)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("client_id", _clientId);
                dict.Add("client_secret", _clientSecret);
                dict.Add("grant_type", "client_credentials");
                var content = new FormUrlEncodedContent(dict);

                var response = await _httpClient.PostAsync("v2020-07/auth/token/", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                    Console.ReadLine();
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                _authToken = JsonConvert.DeserializeObject<AuthToken>(resultJson);
            }          

            return _authToken;
        }
    }

    class AuthToken
    {
        public AuthToken()
        {
            Issued = DateTime.UtcNow;
        }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(".issued")]
        public DateTime Issued { get; set; }

        [JsonProperty(".expires")]
        public DateTime Expires
        {
            get { return Issued.AddSeconds(ExpiresIn); }
        }
    }

    class EveractiveRspWrapper<T>
    {
        public T Data { get; set; }
        
        #nullable enable
        public PaginationInfo? PaginationInfo { get; set; }
    }

    class PaginationInfo
    {
        int Page { get; set; }
        int PageSize { get; set; }
        int TotalItems { get; set; }
        int TotalPages { get; set; }
    }

    class SteamTrap
    {
        [JsonProperty("id")]
        string Id { get; set; }
        
        [JsonProperty("status")]
        SteamTrapStatus? Status { get; set; }
        
        [JsonProperty("trapDetail")]
        SteamTrapDetail? TrapDetail { get; set; }
    }

    class SteamTrapDetail
    {
        [JsonProperty("manufacturer")]
        string Manufacturer { get; set; }

        [JsonProperty("Model")]
        string Model { get; set; }

        [JsonProperty("orificeDiameter")]
        double? OrificeDiameter { get; set; }

        [JsonProperty("type")]
        string Type { get; set; }
    }

    class SteamTrapStatus
    {
        [JsonProperty("steamState")]
        string? SteamState { get; set; }

        [JsonProperty("steamStateChangeTimestamp")]
        Int64? SteamStateChangeTimestamp { get; set; }

        [JsonProperty("trapState")]
        string? TrapState { get; set; }

        [JsonProperty("trapFailureTimestamp")]
        Int64? TrapFailureTimestamp { get; set; }
    }
}
