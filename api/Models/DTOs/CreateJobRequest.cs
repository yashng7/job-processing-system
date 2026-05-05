using System.ComponentModel.DataAnnotations;

namespace JobProcessing.Api.Models.DTOs;

public class CreateJobRequest
{
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Payload { get; set; } = string.Empty;
}