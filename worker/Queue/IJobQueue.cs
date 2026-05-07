namespace JobProcessing.Worker.Queue;

public interface IJobQueue
{
    Task<Guid?> DequeueAsync(CancellationToken cancellationToken = default);
    Task RequeueAsync(Guid jobId, CancellationToken cancellationToken = default);
}