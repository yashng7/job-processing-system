using JobProcessing.Worker.Data;
using JobProcessing.Worker.Queue;
using JobProcessing.Worker.Repositories;
using JobProcessing.Worker.Services;
using JobProcessing.Worker.Workers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console(new CompactJsonFormatter())
            .Enrich.FromLogContext();
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");

        var redisConnection = context.Configuration["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Redis connection string not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddSingleton<IJobQueue>(sp =>
            new RedisJobQueue(redisConnection, sp.GetRequiredService<ILogger<RedisJobQueue>>()));

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobProcessorService, JobProcessorService>();

        services.AddHostedService<JobWorker>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Worker service starting up...");

await host.RunAsync();