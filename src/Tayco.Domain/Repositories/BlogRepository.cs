using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tayco.Domain.Model;

namespace Tayco.Domain.Repositories
{
    internal class BlogRepository : IBlogRepository
    {
        private readonly HttpClient _client;
        private readonly string _blogsEndpoint;

        public BlogRepository(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _blogsEndpoint = configuration["BlogsEndpoint:BaseUrl"];
        }

        public async Task<IEnumerable<Blog>> GetAllAsync()
        {
            var response = await _client.GetAsync($"{_blogsEndpoint}index.json");
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<Blog[]>(stream);
        }

        public Task<string> GetBlogContentAsync(string id)
        {
            return _client.GetStringAsync($"{_blogsEndpoint}{id}.md");
        }
    }
}
