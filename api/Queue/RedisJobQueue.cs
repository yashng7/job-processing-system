using StackExchange.Redis;

namespace JobProcessing.Api.Queue;

public class RedisJobQueue : IJobQueue
{
    private const string QueueKey = "job:queue";
    private readonly IDatabase _database;
    private readonly ILogger<RedisJobQueue> _logger;

    public RedisJobQueue(string connectionString, ILogger<RedisJobQueue> logger)
    {
        _logger = logger;
        var multiplexer = ConnectionMultiplexer.Connect(connectionString);
        _database = multiplexer.GetDatabase();
    }

    public async Task EnqueueAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        await _database.ListLeftPushAsync(QueueKey, jobId.ToString());
        _logger.LogInformation("Job {JobId} enqueued to Redis queue", jobId);
    }
}