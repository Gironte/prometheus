using System.Net.Http;
using System.Threading.Tasks;

namespace Avia.Services
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Get(string address)
        {
            var result = await _httpClient.GetAsync(address);

            return await result.Content.ReadAsStringAsync();
        }

        private readonly HttpClient _httpClient;
    }
}