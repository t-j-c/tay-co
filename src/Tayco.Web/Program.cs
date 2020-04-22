using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tayco.Web.Services;

namespace Tayco.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            ConfigureApp(builder.Configuration);
            ConfigureServices(builder.Services);
            await builder.Build().RunAsync();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<BlogService>();
            return services;
        }

        private static IConfigurationBuilder ConfigureApp(IConfigurationBuilder configuration)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
#if DEBUG
                { "BlogsEndpoint:BaseUrl", "blogs/" }
#else
                { "BlogsEndpoint:BaseUrl", "https://s3.us-east-2.amazonaws.com/blogs.tay-co.com/" }
#endif
            });
            return configuration;
        }
    }
}
