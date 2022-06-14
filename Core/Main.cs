using System;
using System.Collections.Generic;

namespace DefiCalc.Core
{
    public static class Main
    {
        public static EventHandler<DayCalculatedEventArgs> DayCalculated;
        public static EventHandler<DateTime> DateChanged;

        public static double Calc(int days, double initialPrinciple, List<InvestmentSchedule> schedules)
        {
            var total = initialPrinciple;
            var simple = 0D;

            for (var i = 0; i < days; i++)
            {
                var date = DateTime.Today.AddDays(i + 1);
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

                total += additional;
                total -= withdrawn;

                total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
                DayCalculated?.Invoke(null, new DayCalculatedEventArgs
                {
                    Day = i + 1,
                    InterestRate = interest,
                    AmountToAdd = add,
                    AmountPendingExtraction = simple,
                    AmountExtracted = extracted,
                    AmountWithdrawn = withdrawn,
                    AdditionalInvestment = additional, 
                    Total = total
                });
            }

            return total;
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