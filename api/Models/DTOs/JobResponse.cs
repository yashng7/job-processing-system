namespace JobProcessing.Api.Models.DTOs;

public class JobResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? Result { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static JobResponse FromJob(Job job) => new()
    {
        Id = job.Id,
        Name = job.Name,
        Status = job.Status.ToString(),
        Payload = job.Payload,
        Result = job.Result,
        RetryCount = job.RetryCount,
        CreatedAt = job.CreatedAt,
        UpdatedAt = job.UpdatedAt
    };
}