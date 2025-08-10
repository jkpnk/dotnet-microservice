namespace CommandsService.EventProcessing
{
    public interface IEventProcessor
    {
        void ProcessorEvent(string message);
        
    }
}