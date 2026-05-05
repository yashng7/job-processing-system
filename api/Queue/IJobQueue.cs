namespace JobProcessing.Api.Queue;

public interface IJobQueue
{
    Task EnqueueAsync(Guid jobId, CancellationToken cancellationToken = default);
}