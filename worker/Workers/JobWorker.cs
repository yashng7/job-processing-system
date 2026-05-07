using JobProcessing.Worker.Models;
using JobProcessing.Worker.Queue;
using JobProcessing.Worker.Repositories;
using JobProcessing.Worker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JobProcessing.Worker.Workers;

public class JobWorker : BackgroundService
{
    private const int MaxRetries = 3;
    private const int PollingIntervalMs = 1000;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<JobWorker> _logger;

    public JobWorker(
        IServiceScopeFactory scopeFactory,
        IJobQueue jobQueue,
        ILogger<JobWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _jobQueue = jobQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Worker started. Listening for jobs...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var jobId = await _jobQueue.DequeueAsync(stoppingToken);

                if (jobId is null)
                {
                    await Task.Delay(PollingIntervalMs, stoppingToken);
                    continue;
                }

                await ProcessJobAsync(jobId.Value, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in worker loop. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Job Worker stopped.");
    }

    private async Task ProcessJobAsync(Guid jobId, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        var processor = scope.ServiceProvider.GetRequiredService<IJobProcessorService>();

        var job = await repository.GetByIdAsync(jobId, stoppingToken);
        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found in database. Skipping.", jobId);
            return;
        }

        _logger.LogInformation("Starting job {JobId} | RetryCount: {RetryCount}", job.Id, job.RetryCount);

        job.Status = JobStatus.Processing;
        await repository.UpdateAsync(job, stoppingToken);

        try
        {
            var (success, result) = await processor.ProcessAsync(job, stoppingToken);

            if (success)
            {
                job.Status = JobStatus.Completed;
                job.Result = result;
                _logger.LogInformation("Job {JobId} marked as Completed", job.Id);
            }
            else
            {
                await HandleFailureAsync(job, result, repository, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            job.Status = JobStatus.Queued;
            await repository.UpdateAsync(job, stoppingToken);
            _logger.LogWarning("Job {JobId} cancelled and requeued", job.Id);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} threw an exception", job.Id);
            await HandleFailureAsync(job, ex.Message, repository, stoppingToken);
        }

        await repository.UpdateAsync(job, stoppingToken);
    }

    private async Task HandleFailureAsync(
        Job job,
        string reason,
        IJobRepository repository,
        CancellationToken stoppingToken)
    {
        job.RetryCount++;

        if (job.RetryCount < MaxRetries)
        {
            job.Status = JobStatus.Queued;
            job.Result = $"Attempt {job.RetryCount} failed: {reason}. Retrying...";
            await repository.UpdateAsync(job, stoppingToken);
            await _jobQueue.RequeueAsync(job.Id, stoppingToken);
            _logger.LogWarning("Job {JobId} failed. Retrying (attempt {Attempt}/{Max})",
                job.Id, job.RetryCount, MaxRetries);
        }
        else
        {
            job.Status = JobStatus.Failed;
            job.Result = $"Job permanently failed after {MaxRetries} attempts. Last error: {reason}";
            _logger.LogError("Job {JobId} permanently failed after {MaxRetries} retries", job.Id, MaxRetries);
        }
    }
}