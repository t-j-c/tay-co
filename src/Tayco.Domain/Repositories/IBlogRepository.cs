using System.Collections.Generic;
using System.Threading.Tasks;
using Tayco.Domain.Model;

namespace Tayco.Domain.Repositories
{
    public interface IBlogRepository
    {
        Task<IEnumerable<Blog>> GetAllAsync();
        Task<string> GetBlogContentAsync(string id);
    }
}
