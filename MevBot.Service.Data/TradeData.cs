namespace MevBot.Service.Data
{
    public class TradeData
    {
        public long SlotNumber { get; set; }
        public decimal AmountIn { get; set; }
        public decimal AmountOut { get; set; }
        public decimal ExpectedAmountOut { get; set; }
        public decimal SourceTokenChange { get; set; }
        public decimal DestinationTokenChange { get; set; }
        public decimal CommissionAmount { get; set; }
        public string? CommissionDirection { get; set; }
        public List<string>? LiquidityPoolAddress { get; set; }
        public List<string>? ExchangeMarketAddress { get; set; }
        public decimal BeforeSourceBalance { get; set; }
        public decimal BeforeDestinationBalance { get; set; }
        public decimal MinimumReturn { get; set; }
        public ulong GasFee { get; set; }
        public string? TokenMintAddress { get; set; }
        public string? BuyerPublicKey { get; set; }
        public decimal AmountToBuy { get; set; }
        public decimal AmountToSell { get; set; }

        // TODO: research if compute budget is required
        // public decimal ComputeBudget { get; set; }

        public decimal NetworkLiquidity { get; set; }
        public decimal Slippage { get; set; }

        // enrich data
        public string? BlockTime { get; set; }
        public decimal TransactionFee { get; set; }
        public long ComputeUnitsConsumed { get; set; }



        // Rule results
        public bool SlippagePassed { get; set; }
        public bool SequenceAndTimingPassed { get; set; }
        public bool GasFeePassed { get; set; }
        public bool DexLiquidityPassed { get; set; }

    }
}
