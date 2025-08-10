using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeAsync().GetAwaiter().GetResult();
        }


        private async Task InitializeAsync()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["ConnectionStrings:RabbitMQHost"],
                Port = int.Parse(_configuration["ConnectionStrings:RabbitMQPort"]!)
            };

            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex}");
            }
        }

        public void PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);
            if (_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMq Connection Open, sending message");
                // todo send the message
                SendMessageAsync(message).GetAwaiter().GetResult();
            }
            else
            {
                Console.WriteLine("--> RabbitMQ Connection shutdown");
            }
        }

        public void Dispose()
        {
            Console.WriteLine("MessageBus Disposed");
            if (_channel.IsOpen)
            {
                _channel.CloseAsync();
                _connection.CloseAsync();
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }

        private async Task SendMessageAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(
                exchange: "trigger",
                routingKey: "",
                mandatory: false,
                basicProperties: new BasicProperties(),       // nếu có
                body: body // nếu muốn

            );
            Console.WriteLine($"--> We have sent {message}");
        }
    }
}