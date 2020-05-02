using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tayco.Domain.Model;
using Tayco.Domain.Repositories;
using Tayco.Domain.Services;
using Xunit;

namespace Tayco.Domain.UnitTests.Services
{
    public class BlogServiceTests
    {
        [Fact]
        public async Task GetBlogsAsync_ReturnsBlogs()
        {
            var blogs = new List<Blog>
            {
                new Blog {Id = Guid.NewGuid().ToString()},
                new Blog {Id = Guid.NewGuid().ToString()}
            };

            var repository = new Mock<IBlogRepository>();
            repository.Setup(r => r.GetAllAsync()).ReturnsAsync(blogs);
            var service = new BlogService(repository.Object);

            var result = await service.GetBlogsAsync();
            result.ShouldBe(blogs);
        }
    }
}
