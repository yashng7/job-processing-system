using JobProcessing.Api.Models.DTOs;
using JobProcessing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobProcessing.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobService jobService, ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<JobResponse>> CreateJob(
        [FromBody] CreateJobRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var job = await _jobService.CreateJobAsync(request, cancellationToken);
        _logger.LogInformation("Job {JobId} created via API", job.Id);
        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobResponse>>> GetJobs(CancellationToken cancellationToken)
    {
        var jobs = await _jobService.GetAllJobsAsync(cancellationToken);
        return Ok(jobs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobResponse>> GetJob(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetJobByIdAsync(id, cancellationToken);
        if (job is null) return NotFound(new { message = $"Job {id} not found." });
        return Ok(job);
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult<RetryJobResponse>> RetryJob(Guid id, CancellationToken cancellationToken)
    {
        var result = await _jobService.RetryJobAsync(id, cancellationToken);
        if (result is null) return NotFound(new { message = $"Job {id} not found." });
        return Ok(result);
    }
}