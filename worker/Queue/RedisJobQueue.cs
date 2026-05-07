using StackExchange.Redis;

namespace JobProcessing.Worker.Queue;

public class RedisJobQueue : IJobQueue
{
    private const string QueueKey = "job:queue";
    private const int BlockingTimeoutSeconds = 5;
    private readonly IDatabase _database;
    private readonly ILogger<RedisJobQueue> _logger;

    public RedisJobQueue(string connectionString, ILogger<RedisJobQueue> logger)
    {
        _logger = logger;
        var multiplexer = ConnectionMultiplexer.Connect(connectionString);
        _database = multiplexer.GetDatabase();
    }

    public async Task<Guid?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _database.ListRightPopAsync(QueueKey);
            if (result.IsNullOrEmpty) return null;

            if (Guid.TryParse(result, out var jobId))
            {
                _logger.LogInformation("Dequeued job {JobId} from Redis", jobId);
                return jobId;
            }

            _logger.LogWarning("Failed to parse job ID from queue: {Value}", result);
            return null;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Error dequeuing from Redis");
            return null;
        }
    }

    public async Task RequeueAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        await _database.ListLeftPushAsync(QueueKey, jobId.ToString());
        _logger.LogInformation("Job {JobId} requeued to Redis", jobId);
    }
}