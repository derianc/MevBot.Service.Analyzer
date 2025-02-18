using MevBot.Service.Analyzer.models;
using StackExchange.Redis;
using System.Text.Json;

namespace MevBot.Service.Analyzer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _redisDb;

        private readonly string _splTokenAddress;
        private readonly string _redisAnalyzeQueue = "solana_analyze_queue";
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
                RedisValue message = await _redisDb.ListRightPopAsync(_redisAnalyzeQueue);

                if (message.HasValue)
                {
                    try
                    {
                        // Deserialize the message into our LogsNotificationResponse object.
                        var logsNotification = JsonSerializer.Deserialize<LogsNotificationResponse>(message);

                        // Log the message received.
                        _logger.LogInformation("{time} - Received message from Redis queue: {message}", DateTimeOffset.Now, message);

                        // only process the message if it meets the criteria for a sandwich opportunity
                        if (IsSandwichOpportunity(logsNotification))
                        {
                            _logger.LogInformation("{time} - Sandwich opportunity detected in message: {message}", DateTimeOffset.Now, message);

                            // push message to redis
                            await _redisDb.ListLeftPushAsync(_redisBuyQueue, message);
                            _logger.LogInformation("{time} - Pushed logsNotification to Redis queue: {queueName}", DateTimeOffset.Now, _redisBuyQueue);

                            // Additional processing logic for a sandwich opportunity can be added here.
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{time} - Error processing message from Redis queue: {message}, {error}", DateTimeOffset.Now, message, ex.Message);
                    }
                }
                else
                {
                    _logger.LogDebug("{time} - No messages in Redis queue", DateTimeOffset.Now);
                }
            }
        }

        /// <summary>
        /// Dynamically reads the current SPL token addresses from environment variables and
        /// analyzes the logs to determine if a sandwich opportunity exists.
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

            // Split into individual tokens and trim whitespace.
            var tokenList = _splTokenAddress
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(token => token.Trim())
                                .ToList();

            // If no tokens are provided, return false.
            if (!tokenList.Any())
            {
                return false;
            }

            // Check if any token from the list appears in the logs.
            bool containsSplToken = tokenList.Any(token => logs.Any(log => log.Contains(token, StringComparison.OrdinalIgnoreCase)));

            // Also check that the logs contain the keyword "swap".
            bool containsSwap = logs.Any(log => log.Contains("swap", StringComparison.OrdinalIgnoreCase));

            return containsSplToken && containsSwap;
        }
    }
}

