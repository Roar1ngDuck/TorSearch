using TorSearch.Frontend.Data;

namespace TorSearch.Frontend.Services
{
    public class SearchService
    {
        private readonly HttpClient _client;

        public SearchService(HttpClient client)
        {
            _client = client;
            _client.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<List<DomainInfo>> SearchAsync(string query, int page)
        {
            var url = $"http://backend/search?query={query}&page={page}";
            return await GetAsync(url);
        }

        private async Task<List<DomainInfo>> GetAsync(string uri)
        {
            try
            {
                return await _client.GetFromJsonAsync<List<DomainInfo>>(uri) ?? new List<DomainInfo>();
            }
            catch (Exception)
            {
                // Handle exception or log it
                return new List<DomainInfo>();
            }
        }
    }
}
