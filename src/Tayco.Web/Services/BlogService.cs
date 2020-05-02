using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tayco.Web.Model;

namespace Tayco.Web.Services
{
    public class BlogService
    {
        private readonly HttpClient _client;
        private readonly string _blogsEndpoint;
        private Blog[] _blogs;

        public BlogService(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _blogsEndpoint = configuration["BlogsEndpoint:BaseUrl"];
        }

        public async Task<IEnumerable<Blog>> GetBlogsAsync()
        {
            await LoadBlogsAsync();
            return _blogs;
        }

        public async Task<Blog> FindAsync(string id)
        {
            await LoadBlogsAsync();
            return _blogs.FirstOrDefault(b => b.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public Task<string> GetBlogContent(string id)
        {
            return _client.GetStringAsync($"{_blogsEndpoint}{id}.md");
        }

        private async ValueTask LoadBlogsAsync()
        {
            if (_blogs != null) return;

            var response = await _client.GetAsync($"{_blogsEndpoint}index.json");
            var stream = await response.Content.ReadAsStreamAsync();
            _blogs = await JsonSerializer.DeserializeAsync<Blog[]>(stream);
            
            foreach (var blog in _blogs)
            {
                blog.Next = _blogs.OrderBy(b => b.UploadDate).FirstOrDefault(b => b.UploadDate > blog.UploadDate);
                blog.Previous = _blogs.OrderByDescending(b => b.UploadDate).FirstOrDefault(b => b.UploadDate < blog.UploadDate);
            }
        }
    }
}
