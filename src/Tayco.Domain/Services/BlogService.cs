using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tayco.Domain.Model;
using Tayco.Domain.Repositories;

namespace Tayco.Domain.Services
{
    public class BlogService
    {
        private readonly IBlogRepository _repository;
        private Blog[] _blogs;

        public BlogService(IBlogRepository repository)
        {
            _repository = repository;
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

        public Task<string> GetBlogContentAsync(string id)
        {
            return _repository.GetBlogContentAsync(id);
        }

        private async ValueTask LoadBlogsAsync()
        {
            if (_blogs != null) return;

            _blogs = (await _repository.GetAllAsync()).ToArray();
            
            foreach (var blog in _blogs)
            {
                blog.Next = _blogs.OrderBy(b => b.UploadDate).FirstOrDefault(b => b.UploadDate > blog.UploadDate);
                blog.Previous = _blogs.OrderByDescending(b => b.UploadDate).FirstOrDefault(b => b.UploadDate < blog.UploadDate);
            }
        }
    }
}
