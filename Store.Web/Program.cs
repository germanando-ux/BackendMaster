using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Store.Web.Services;
using Store.Web;
using Store.Web.Handlers;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// Configuración del HttpClient con el manejador de autenticación para que todas las llamadas a la API incluyan el token automáticamente y manejen los errores de autenticación de forma centralizada.
// 1. Registramos el manejador (asegúrate de tener el using Store.Web.Handlers;)
builder.Services.AddTransient<AutenticacionHandler>();

// 2. Registramos el HttpClient y le añadimos el manejador
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7260/");
})
.AddHttpMessageHandler<AutenticacionHandler>();

// 3. Sustituimos tu línea anterior para que el sistema use siempre este cliente configurado
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7260/") });
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();
