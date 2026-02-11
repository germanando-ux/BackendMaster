using MassTransit;
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

    // --- CONFIGURACIÓN DE CORS ---
    builder.Services.AddCors(options =>
    {
        options.AddPolicy( "AllowBlazor", policy =>
        {
            //policy.WithOrigins("https://localhost:7072","https://store_web.dev.localhost:7072") // La URL de tu Front
            // Esta línea es la clave: permite cualquier puerto que venga de localhost
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost") // Permite cualquier cosa que sea localhost
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

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

    // --- CONFIGURACIÓN DE MENSAJERÍA ASÍNCRONA (RabbitMQ) ---


    // Configuración de MassTransit para gestionar la comunicación con RabbitMQ.
    // MassTransit actúa como un "Bus de Servicio" que simplifica el envío de mensajes.   
    builder.Services.AddMassTransit(x =>
    {
        // 1. Configurar el Outbox
        x.AddEntityFrameworkOutbox<StoreDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox(); // Habilita el envío automático desde la tabla
        });

        // 2. Configurar RabbitMQ
        x.UsingRabbitMq((context, cfg) =>
        {
            // Obtener el host del appsettings o usar el nombre del servicio en docker-compose
            var rabbitHost = builder.Configuration["MassTransit:Host"] ?? "rabbitmq_bus";

            cfg.Host(rabbitHost, "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ConfigureEndpoints(context);
        });
    });


    var app = builder.Build();

    app.UseCors("AllowBlazor");

    app.UseRouting(); 


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

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthorization();

    app.MapControllers();

    //Lanza migraciones automáticamente
    //using (var scope = app.Services.CreateScope())
    //{
    //    var services = scope.ServiceProvider;
    //    var context = services.GetRequiredService<StoreDbContext>();
    //    context.Database.Migrate();
    //}

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