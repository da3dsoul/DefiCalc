using System;
using System.Collections.Generic;

namespace DefiCalc.Core
{
    public static class Main
    {
        public static EventHandler<DayCalculatedEventArgs> DayCalculated;
        public static EventHandler<DateTime> DateChanged;

        public static double Calc(int days, int reinvestmentPeriod, double reinvestmentAmount, int reinvestmentOffset,
            double initialPrinciple)
        {
            return Calc(days, initialPrinciple,
                new List<InvestmentSchedule>
                {
                    new()
                    {
                        Amount = reinvestmentAmount, Period = reinvestmentPeriod,
                        StartDate = DateTime.Today.AddDays(reinvestmentOffset)
                    }
                });
        }

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
                var withdrawn = 0D;
                if (add + simple >= 10D)
                {
                    total += add + simple;
                    withdrawn = add + simple;
                    simple = 0D;
                }
                else
                    simple += add;

                var additional = 0D;
                foreach (var schedule in schedules)
                {
                    if (date < schedule.StartDate || (schedule.EndDate != null && date >= schedule.EndDate.Value)) continue;
                    if (schedule.Period != 0 && (date - schedule.StartDate).TotalDays % schedule.Period != 0) continue;
                    additional += schedule.Amount;
                }

                total += additional;

                total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
                DayCalculated?.Invoke(null, new DayCalculatedEventArgs
                {
                    Day = i + 1,
                    InterestRate = interest,
                    AmountToAdd = add,
                    AmountPendingWithdraw = simple,
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