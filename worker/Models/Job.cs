namespace JobProcessing.Worker.Models;

public class Job
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string? Result { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}