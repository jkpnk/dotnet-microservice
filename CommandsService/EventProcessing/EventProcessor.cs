using System.Text.Json;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventProcessor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public void ProcessorEvent(string message)
        {
            var eventType = DetermindEvent(message);

            switch (eventType)
            {
                case EventType.PlatFromPublished:
                    Console.WriteLine($"Reslove event platform publish");
                    addPlatform(message);
                    return;
                default:
                    Console.WriteLine($"Could not determine the event type");
                    return;
            }
        }

        private EventType DetermindEvent(string notificationMessage)
        {
            Console.WriteLine($"--> Detewrmining Event");
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (eventType.Event)
            {
                case "Platform_Published":
                    Console.WriteLine($"Platform public event Detected");
                    return EventType.PlatFromPublished;
                default:
                    Console.WriteLine($"Could not determine the event type");
                    return EventType.Undetermined;
            }
        }

        private void addPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
                var platformPublishDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try
                {
                    var plat = new Platform
                    {
                        ExternalID = platformPublishDto.Id,
                        Name = platformPublishDto.Name,
                    };

                    if (!repo.ExternalPlatformExists(plat.ExternalID))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine($"--> Platform already exists...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not add Platform to Db {ex.Message}");
                    throw;
                }
            }
        }

        enum EventType
        {
            PlatFromPublished,
            Undetermined
        }
    }
}