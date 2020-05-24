using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Tayco.Domain;

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
            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            await builder.Build().RunAsync();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddDomainServices();
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
