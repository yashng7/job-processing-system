using JobProcessing.Api.Models;

namespace JobProcessing.Api.Repositories;

public interface IJobRepository
{
    Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default);
}