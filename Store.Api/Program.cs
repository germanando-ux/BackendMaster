using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using Scalar.AspNetCore;
using Store.Api.Mappings;
using Store.Api.Middlewares;
using Store.Data.Data;
using Store.Data.Repositories;
using Store.Data.Service;


// 1. ESTA LÍNEA DEBE SER LA PRIMERA (después de los using)
// Es la que crea la variable 'logger' que el catch usará después.
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();




try
{
    var builder = WebApplication.CreateBuilder(args);
    // --- CONFIGURACIÓN DE NLOG ---
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    //1. REGISTRO DE SERVICIOS
    // Configuración de Redis
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        // Si corres la API desde Visual Studio (Windows), usa "localhost:6379"
        // Si corres la API en Docker, usa "redis_cache:6379"
        options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
        options.InstanceName = "Store_";
    });


    builder.Services.AddScoped<ICacheService, CacheService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    builder.Services.AddControllers()
    .AddJsonOptions(options => //evita bucles infinitos en las listas relacionadas de los DTOs de entityframework
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

    //automaper
    builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);


    //MANEJADOR DE ERRORES
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails(); // Estándar para errores en APIs

    // AQUÍ CONECTAMOS EF CORE CON POSTGRESQL
    builder.Services.AddDbContext<StoreDbContext>(options =>options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // Registramos la Unidad de Trabajo con un ciclo de vida 'Scoped'.
    // Esto significa que se crea una instancia por cada petición HTTP y se destruye al finalizar.
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
    app.MapOpenApi(); // Genera el documento JSON de la API
    app.MapScalarApiReference(); // Activa la interfaz visual en /scalar/v1
    }

    // 2. ACTIVAR el middleware (Debe ir antes de los controladores)
    app.UseExceptionHandler();

    // 3. ¡IMPORTANTE! Este middleware captura los return NotFound() y los convierte 
    // al formato estandarizado de ProblemDetails antes de salir.
    app.UseStatusCodePages();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "La aplicación se detuvo por una excepción crítica");
    throw;
}
finally
{
    LogManager.Shutdown();
}