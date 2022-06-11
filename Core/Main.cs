using System;
using System.Collections.Generic;

namespace DefiCalc.Core
{
    public static class Main
    {
        public static EventHandler<DayCalculatedEventArgs> DayCalculated;
        public static double Calc(int days, int reinvestmentPeriod, double reinvestmentAmount, int reinvestmentOffset, double initialPrinciple)
        {
            var total = initialPrinciple;
            var simple = 0D;

            for (var i = 0; i < days; i++)
            {
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

                // we do this at the end of the day, as it does apply until the next period
                if (i >= reinvestmentOffset && (i - reinvestmentOffset) % reinvestmentPeriod == 0)
                {
                    total += reinvestmentAmount;
                }

                total = Math.Round(total, 2, MidpointRounding.AwayFromZero);
                DayCalculated?.Invoke(null, new DayCalculatedEventArgs
                {
                    Day = i + 1,
                    InterestRate = interest,
                    AmountToAdd = add,
                    AmountPendingWithdraw = simple,
                    AmountWithdrawn = withdrawn,
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