// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace DefiCalc.Core.Models
{
    public record InvestmentSchedule : DateRange
    {
        /// <summary>
        /// The time between investments, in days
        /// </summary>
        public int Period { get; init; }
        /// <summary>
        /// Amount to invest
        /// </summary>
        public double Amount { get; init; }
        /// <summary>
        /// Amount to Withdraw
        /// </summary>
        public double WithdrawAmount { get; init; }
    }
}