using JobProcessing.Api.Hubs;
using JobProcessing.Api.Models;
using JobProcessing.Api.Models.DTOs;
using JobProcessing.Api.Queue;
using JobProcessing.Api.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace JobProcessing.Api.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _repository;
    private readonly IJobQueue _queue;
    private readonly IHubContext<JobHub> _hubContext;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IJobRepository repository,
        IJobQueue queue,
        IHubContext<JobHub> hubContext,
        ILogger<JobService> logger)
    {
        _repository = repository;
        _queue = queue;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<JobResponse> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
    {
        var job = new Job
        {
            Name = request.Name,
            Payload = request.Payload,
            Status = JobStatus.Queued
        };

        var created = await _repository.CreateAsync(job, cancellationToken);
        _logger.LogInformation("Job {JobId} created with name '{JobName}'", created.Id, created.Name);

        await _queue.EnqueueAsync(created.Id, cancellationToken);

        var response = JobResponse.FromJob(created);
        await _hubContext.Clients.All.SendAsync("JobCreated", response, cancellationToken);

        return response;
    }

    public async Task<IEnumerable<JobResponse>> GetAllJobsAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await _repository.GetAllAsync(cancellationToken);
        return jobs.Select(JobResponse.FromJob);
    }

    public async Task<JobResponse?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetByIdAsync(id, cancellationToken);
        return job is null ? null : JobResponse.FromJob(job);
    }

    public async Task<RetryJobResponse?> RetryJobAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _repository.GetByIdAsync(id, cancellationToken);
        if (job is null) return null;

        if (job.Status != JobStatus.Failed)
        {
            return new RetryJobResponse
            {
                JobId = id,
                Message = $"Job cannot be retried. Current status: {job.Status}"
            };
        }

        job.Status = JobStatus.Queued;
        job.Result = null;
        await _repository.UpdateAsync(job, cancellationToken);

        await _queue.EnqueueAsync(job.Id, cancellationToken);

        _logger.LogInformation("Job {JobId} manually queued for retry", job.Id);

        var jobResponse = JobResponse.FromJob(job);
        await _hubContext.Clients.All.SendAsync("JobUpdated", jobResponse, cancellationToken);

        return new RetryJobResponse
        {
            JobId = id,
            Message = "Job has been queued for retry."
        };
    }
}