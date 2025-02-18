using MevBot.Service.Analyzer.models;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace MevBot.Service.Analyzer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _redisDb;

        private readonly string _splTokenAddress;
        private readonly string _redisQueueName = "solana_logs_queue";
        private readonly string _redisBuyQueue = "solana_buy_queue";
        private readonly string _redisConnectionString;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _splTokenAddress = _configuration.GetValue<string>("Solana:SPL_TOKEN_ADDRESS") ?? string.Empty;
            _redisConnectionString = _configuration.GetValue<string>("Redis:REDIS_URL") ?? string.Empty;

            // Connect to Redis.
            var options = ConfigurationOptions.Parse(_redisConnectionString);
            _redis = ConnectionMultiplexer.Connect(options);
            _redisDb = _redis.GetDatabase();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{time} - Starting Solana MEV Bot Analyzer", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Try to pop a message from the Redis queue.
                RedisValue message = await _redisDb.ListRightPopAsync(_redisQueueName);

                if (message.HasValue)
                {
                    try
                    {
                        // Deserialize the message into our LogsNotificationResponse object.
                        var logsNotification = JsonSerializer.Deserialize<LogsNotificationResponse>(message);

                        // Log the message received.
                        _logger.LogInformation("{time} - Received message from Redis queue: {message}", DateTimeOffset.Now, logsNotification);

                        // Analyze the logs for a viable sandwich transaction.
                        if (IsSandwichOpportunity(logsNotification))
                        {
                            _logger.LogInformation("{time} - Sandwich opportunity detected in message: {message}", DateTimeOffset.Now, logsNotification);

                            // push message to redis
                            await _redisDb.ListLeftPushAsync(_redisBuyQueue, message);
                            _logger.LogInformation("{time} - Pushed logsNotification to Redis queue: {queueName}", DateTimeOffset.Now, _redisBuyQueue);

                            // Additional processing logic for a sandwich opportunity can be added here.
                        }
                        else
                        {
                            //_logger.LogInformation("{time} - No viable sandwich opportunity detected in message.", DateTimeOffset.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{time} - Error processing message from Redis queue: {message}", DateTimeOffset.Now, message);
                    }
                }
                else
                {
                    _logger.LogDebug("{time} - No messages in Redis queue", DateTimeOffset.Now);
                }

                // Wait for 1 second before checking the queue again.
                //await Task.Delay(1000, stoppingToken);
            }
        }

        /// <summary>
        /// Analyzes the logs in the provided LogsNotificationResponse to determine if it is viable for a sandwich transaction.
        /// </summary>
        /// <param name="logsNotification">The deserialized logs notification response from Solana.</param>
        /// <returns>True if the message meets criteria for a sandwich opportunity; otherwise, false.</returns>
        private bool IsSandwichOpportunity(LogsNotificationResponse logsNotification)
        {
            if (logsNotification == null || logsNotification.@params?.result?.value?.logs == null)
            {
                return false;
            }

            var logs = logsNotification.@params.result.value.logs;

            // Example criteria for sandwich opportunity:
            // 1. The logs should contain the SPL token address.
            // 2. The logs should contain indicators of a swap transaction (e.g., the keyword "swap").
            bool containsSplToken = logs.Any(log => log.Contains(_splTokenAddress, StringComparison.OrdinalIgnoreCase));
            bool containsSwap = logs.Any(log => log.Contains("swap", StringComparison.OrdinalIgnoreCase));

            // Additional logic can be implemented to further analyze slippage, compute units, etc.
            return containsSplToken && containsSwap;
        }
    }
}
