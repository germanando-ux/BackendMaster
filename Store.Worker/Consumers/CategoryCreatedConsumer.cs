using MassTransit;
using Store.Domain.Contracts;


namespace Store.Worker.Consumers
{

    /// <summary>
    /// Esta clase es el "obrero". Implementa IConsumer de nuestro contrato.
    /// MassTransit buscará automáticamente esta clase cuando llegue un mensaje de este tipo.
    /// </summary>
    public class CategoryCreatedConsumer : IConsumer<CategoryCreated>
    {
        private readonly ILogger<CategoryCreatedConsumer> _logger;

        public CategoryCreatedConsumer(ILogger<CategoryCreatedConsumer> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Este método se ejecuta cada vez que RabbitMQ nos entrega un mensaje.
        /// </summary>
        /// <param name="context">Contiene el mensaje (context.Message) y herramientas para responder.</param>
         
        public async Task Consume(ConsumeContext<CategoryCreated> context)
        {
            var message = context.Message;

            // Aquí es donde iría la lógica real (ej: enviar un email, registrar en un log externo)
            _logger.LogInformation("Confirmación recibida: Se ha creado la categoría {Name} con ID {Id}",
                message.Name, message.Id);

            // --- SIMULACIÓN DE ERROR ---
            // Imagina que aquí llamas a una DB o API externa y falla.
            if (message.Name.Contains("Error", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("¡Error simulado procesando {Name}!", message.Name);
                throw new Exception("Simulación de caída de servicio externo.");
            }

            _logger.LogInformation("Éxito: Categoría {Name} procesada correctamente.", message.Name);

            // Simulamos un trabajo que tarda un poco
            await Task.Delay(500);
        }
    }
}
