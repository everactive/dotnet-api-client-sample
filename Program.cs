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
            var Sensors = apiService.GetEversensors().Result;

            Console.WriteLine($"Returned Sensors");
            Console.WriteLine(Sensors);
            Console.ReadLine();
        }
    }

    class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();


        private AuthToken _authToken = new AuthToken();
        private string _authUrl;
        private string _clientId;
        private string _clientSecret;

        public ApiService(
            string baseAddress,
            string clientId,
            string clientSecret)
        {
            _authUrl = baseAddress + "/auth/token";
            _clientId = clientId;
            _clientSecret = clientSecret;

            _httpClient.BaseAddress = new Uri(baseAddress);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetEversensors()
        {
            var token = await _getAuthToken();            
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var response = await _httpClient.GetAsync("ds/v1/eversensors");
            var resultJson = await response.Content.ReadAsStringAsync();
            return resultJson;
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

                var response = await _httpClient.PostAsync(_authUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Unsuccessful authentication - {response.StatusCode}");
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
    
}
