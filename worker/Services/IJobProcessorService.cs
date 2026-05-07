using JobProcessing.Worker.Models;

namespace JobProcessing.Worker.Services;

public interface IJobProcessorService
{
    Task<(bool success, string result)> ProcessAsync(Job job, CancellationToken cancellationToken = default);
}