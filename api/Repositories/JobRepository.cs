using JobProcessing.Api.Data;
using JobProcessing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobProcessing.Api.Repositories;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
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