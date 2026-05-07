using JobProcessing.Worker.Models;

namespace JobProcessing.Worker.Services;

public class JobProcessorService : IJobProcessorService
{
    private readonly ILogger<JobProcessorService> _logger;
    private static readonly Random _random = new();

    public JobProcessorService(ILogger<JobProcessorService> logger)
    {
        _logger = logger;
    }

    public async Task<(bool success, string result)> ProcessAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing job {JobId} | Name: {JobName} | Payload: {Payload}",
            job.Id, job.Name, job.Payload);

        await Task.Delay(TimeSpan.FromSeconds(_random.Next(2, 6)), cancellationToken);

        var shouldFail = _random.Next(0, 10) < 2;

        if (shouldFail && job.RetryCount < 2)
        {
            _logger.LogWarning("Job {JobId} simulated failure (retry {RetryCount})", job.Id, job.RetryCount + 1);
            return (false, $"Simulated processing failure on attempt {job.RetryCount + 1}");
        }

        var result = $"Job '{job.Name}' completed successfully at {DateTime.UtcNow:O}. Processed payload: {job.Payload}";
        _logger.LogInformation("Job {JobId} completed successfully", job.Id);
        return (true, result);
    }
}