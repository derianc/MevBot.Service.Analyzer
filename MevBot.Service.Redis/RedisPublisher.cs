using StackExchange.Redis;

namespace MevBot.Service.Redis
{
    public class RedisPublisher
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisPublisher(string redisConnectionString)
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            _redis = ConnectionMultiplexer.Connect(options);
            _db = _redis.GetDatabase();
        }

        public async Task PublishMessageAsync(string message, string channelName)
        {
            Console.WriteLine($"Publishing to Redis: {message}");
            await _db.ListLeftPushAsync(channelName, message);
        }
    }
}
