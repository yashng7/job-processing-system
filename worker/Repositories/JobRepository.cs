using JobProcessing.Worker.Data;
using JobProcessing.Worker.Models;

namespace JobProcessing.Worker.Repositories;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        job.UpdatedAt = DateTime.UtcNow;
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }
}