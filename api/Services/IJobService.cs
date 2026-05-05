using JobProcessing.Api.Models.DTOs;

namespace JobProcessing.Api.Services;

public interface IJobService
{
    Task<JobResponse> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobResponse>> GetAllJobsAsync(CancellationToken cancellationToken = default);
    Task<JobResponse?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RetryJobResponse?> RetryJobAsync(Guid id, CancellationToken cancellationToken = default);
}