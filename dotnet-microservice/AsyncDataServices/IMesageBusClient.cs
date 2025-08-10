using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto);

    }
}