using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace  CommandsService.AsyncDataServices
{
    public class MessageBusSubcriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IChannel _channel;
        private string _queueName;

        public MessageBusSubcriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
            InitializeRabbitMQ().GetAwaiter().GetResult();
        }

        private async Task InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["ConnectionStrings:RabbitMQHost"],
                Port = int.Parse(_configuration["ConnectionStrings:RabbitMQPort"])
            };
            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
                var queueDeclareOk = await _channel.QueueDeclareAsync();
                _queueName = queueDeclareOk.QueueName;
                await _channel.QueueBindAsync(queue: _queueName, exchange: "trigger", routingKey: "");
                Console.WriteLine("--> Listening on the Message Bus");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"--> Error intit rabbitMq: {ex.Message}");
            }
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ModuleHandle, ea) =>
            {
                Console.WriteLine("--> Event Received!");
                var body = ea.Body;
                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                _eventProcessor.ProcessorEvent(notificationMessage);
            };

            _ = _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}