
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Store.Web.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddHttpClient("StoreApi", client =>
            {
                client.BaseAddress = new Uri("http://localhost:8080");
            });

            await builder.Build().RunAsync();
        }
    }
}
