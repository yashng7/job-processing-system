namespace JobProcessing.Api.Models.DTOs;

public class RetryJobResponse
{
    public Guid JobId { get; set; }
    public string Message { get; set; } = string.Empty;
}