using System;
using System.Collections.Generic;

namespace DefiCalc.Core
{
    public static class Main
    {
        public static EventHandler<DayCalculatedEventArgs> DayCalculated;
        public static EventHandler<DateTime> DateChanged;

        public static double Calc(DateTime startDate, int days, double initialPrinciple, double initialInvestment, List<InvestmentSchedule> schedules)
        {
            var total = initialPrinciple;
            var simple = 0D;
            var totalInvested = initialInvestment;

            for (var i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i + 1);
                DateChanged?.Invoke(null, date);
                var interest = GetInterestRate(total);
                var add = total * interest;
                var extracted = 0D;
                if (add + simple >= 10D)
                {
                    total += add + simple;
                    extracted = add + simple;
                    simple = 0D;
                }
                else
                    simple += add;

                var additional = 0D;
                var withdrawn = 0D;
                foreach (var schedule in schedules)
                {
                    if (date < schedule.StartDate || (schedule.EndDate != null && date >= schedule.EndDate.Value)) continue;
                    if (schedule.Period != 0 && (date - schedule.StartDate).TotalDays % schedule.Period != 0) continue;
                    if (schedule.Period == 0 && date != schedule.StartDate) continue;
                    additional += schedule.Amount;
                    withdrawn += schedule.WithdrawAmount;
                }

                totalInvested += additional;
                total += additional;
                total -= withdrawn;

                total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
                var taxes = GetTaxes(total - totalInvested);
                var fees = GetFees(total);
                DayCalculated?.Invoke(null, new DayCalculatedEventArgs
                {
                    Day = i + 1,
                    InterestRate = interest,
                    AmountToAdd = add,
                    AmountPendingExtraction = simple,
                    AmountExtracted = extracted,
                    AmountWithdrawn = withdrawn,
                    AdditionalInvestment = additional, 
                    Subtotal = total,
                    Taxes = taxes,
                    Fees = fees,
                    Total = total - taxes - fees
                });
            }

            return total;
        }

        public static double GetTaxes(double total)
        {
            double taxRate;
            double flatTax;
            const double bracket1 = 10275D;
            const double bracket2 = 41775D;
            const double bracket3 = 89075D;
            const double bracket4 = 170050D;
            const double bracket5 = 215950D;
            const double bracket6 = 539900D;
            double bracket;
            switch (total)
            {
                case > bracket6:
                    taxRate = 0.37D;
                    flatTax = 162718D;
                    bracket = bracket6;
                    break;
                case > bracket5:
                    taxRate = 0.35D;
                    flatTax = 49335.50D;
                    bracket = bracket5;
                    break;
                case > bracket4:
                    taxRate = 0.32D;
                    flatTax = 34647.50D;
                    bracket = bracket4;
                    break;
                case > bracket3:
                    taxRate = 0.24D;
                    flatTax = 15213.50D;
                    bracket = bracket3;
                    break;
                case > bracket2:
                    taxRate = 0.22D;
                    flatTax = 4807.50D;
                    bracket = bracket2;
                    break;
                case > bracket1:
                    taxRate = 0.12D;
                    flatTax = 1027.50D;
                    bracket = bracket1;
                    break;
                default:
                    taxRate = 0.10D;
                    flatTax = 0D;
                    bracket = 0D;
                    break;
            }

            return (total - bracket) * taxRate + flatTax;
        }

        public static double GetFees(double total)
        {
            var coinbase = total * 0.01D + 4.95D;
            return coinbase;
        }

        public static double GetInterestRate(double principle)
        {
            return principle switch
            {
                < 100D => 0D,
                < 10000D => 0.02D,
                < 50000D => 0.023D,
                < 100000D => 0.026D,
                < 200000D => 0.034D,
                _ => 0.045D
            };
        }
    }
}