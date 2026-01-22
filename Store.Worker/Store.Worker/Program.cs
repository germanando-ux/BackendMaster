using MassTransit;
using NLog;
using NLog.Web;
using Store.Worker.Consumers;

namespace Store.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // Inicializamos el logger de NLog
            var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                // --- CONFIGURACIÓN DE LOGS (Elastic/NLog) ---
                builder.Logging.ClearProviders();
                builder.Services.AddLogging();
                builder.UseNLog();

                // --- CONFIGURACIÓN DE MASSTRANSIT EN EL WORKER ---
                builder.Services.AddMassTransit(x =>
                {
                    // 1. Registramos el consumidor que creamos antes
                    x.AddConsumer<CategoryCreatedConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        // 2. Configuración automática de la cola
                        // Esto creará una cola en RabbitMQ vinculada a nuestro consumidor
                        // Configuración del endpoint
                        cfg.ReceiveEndpoint("category-created-queue", e =>
                        {
                            // POLÍTICA DE REINTENTOS:
                            // Si falla, espera 5s, luego 10s, luego 30s. Total: 3 intentos extra.
                            e.UseMessageRetry(r => r.Intervals(
                                TimeSpan.FromSeconds(5),
                                TimeSpan.FromSeconds(10),
                                TimeSpan.FromSeconds(30)));

                            e.ConfigureConsumer<CategoryCreatedConsumer>(context);
                        });
                    });
                });

                var host = builder.Build();
                host.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "El Worker de RabbitMQ no pudo arrancar");
                throw;
            }
            finally
            {
                // Cerramos NLog correctamente para asegurar que los últimos logs se envíen a Elastic
                LogManager.Shutdown();
            }
        }
    }
}
