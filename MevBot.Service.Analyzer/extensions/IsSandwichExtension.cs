using MevBot.Service.Data;

namespace MevBot.Service.Analyzer.extensions
{
    public static class IsSandwichExtension
    {
        /// <summary>
        /// Dynamically reads the current SPL token addresses from environment variables and
        /// analyzes the logs to determine if a sandwich opportunity exists.
        /// </summary>
        /// <param name="logsNotification">The deserialized logs notification response from Solana.</param>
        /// <returns>True if the message meets criteria for a sandwich opportunity; otherwise, false.</returns>
        public static bool IsSandwichOpportunity(this SolanaTransaction logsNotification, string tokens)
        {
            if (logsNotification == null || logsNotification.@params?.result?.value?.logs == null)
            {
                return false;
            }

            var logs = logsNotification.@params.result.value.logs;

            // Split into individual tokens and trim whitespace.
            var tokenList = tokens.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(token => token.Trim())
                                  .ToList();

            // If no tokens are provided, return false.
            if (!tokenList.Any())
                return false;

            // Check if any token from the list appears in the logs.
            bool containsSplToken = tokenList.Any(token => logs.Any(log => log.Contains(token, StringComparison.OrdinalIgnoreCase)));

            // Check that the logs contain the keyword "swap" or "buy".
            bool containsSwap = logs.Any(log => log.Contains("swap", StringComparison.OrdinalIgnoreCase));
            bool containsBuy = logs.Any(log => log.Contains("buy", StringComparison.OrdinalIgnoreCase));

            return containsSplToken && (containsBuy || containsSwap);
        }
    }
}
