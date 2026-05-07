namespace JobProcessing.Api.Models;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public string Payload { get; set; } = string.Empty;
    public string? Result { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}