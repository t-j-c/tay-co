﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tayco.Web.Model;

namespace Tayco.Web.Services
{
    public class BlogStateManager
    {
        private readonly HttpClient _client;
        private readonly string _blogsEndpoint;
        private Blog[] _blogs;

        public BlogStateManager(HttpClient client, IConfiguration configuration)
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

        private async ValueTask LoadBlogsAsync()
        {
            if (_blogs != null) return;

            var response = await _client.GetAsync($"{_blogsEndpoint}/index.json");
            var stream = await response.Content.ReadAsStreamAsync();
            _blogs = await JsonSerializer.DeserializeAsync<Blog[]>(stream);
        }
    }
}
