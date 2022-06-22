namespace DefiCalc.Core.Models
{
    public class DayCalculatedEventArgs
    {
        public int Day { get; set; }
        public double InterestRate { get; set; }
        public double AmountToAdd { get; set; }
        public double AmountPendingExtraction { get; set; }
        public double AmountExtracted { get; set; }
        public double AmountWithdrawn { get; set; }
        public double AdditionalInvestment { get; set; }
        public double Subtotal { get; set; }
        public double Taxes { get; set; }
        public double Fees { get; set; }
        public double Total { get; set; }
    }
}