using System.ComponentModel.DataAnnotations;    

namespace JobProcessing.Api.Models.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}