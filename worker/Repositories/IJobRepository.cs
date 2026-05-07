using JobProcessing.Worker.Models;

namespace JobProcessing.Worker.Repositories;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default);
}