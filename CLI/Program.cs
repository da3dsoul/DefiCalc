using System;
using Calc = DefiCalc.Core.Main;

namespace DefiCalc.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var date = DateTime.Now.AddMonths(12);
            var days = (int) Math.Ceiling((date - DateTime.Today).TotalDays) - 1;
            const int reinvestmentPeriod = 14;
            const double reinvestmentAmount = 0D;
            const int reinvestmentOffset = 3;
            const double initialPrinciple = 100D;

            Calc.DayCalculated += (_, eventArgs) =>
            {
                if (initialPrinciple < 500)
                    Console.WriteLine(
                        "Day: {0:D3}, Interest: {1:P1}, PV*I: ${2:N2}, Pending Withdraw: ${3:N2}, Amount Withdrawn: ${4:N2}, Total: ${5:N2}",
                        eventArgs.Day, eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.AmountPendingWithdraw,
                        eventArgs.AmountWithdrawn, eventArgs.Total);
                else
                    Console.WriteLine("Day: {0:D3}, Interest: {1:P1}, PV*I: ${2:N2}, Total: ${3:N2}", eventArgs.Day,
                        eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.Total);
            };
            
            Console.WriteLine("-----Initial Values----------------");
            Console.WriteLine("Days: {0}", days);
            Console.WriteLine("Reinvestment Period: {0} days", reinvestmentPeriod);
            Console.WriteLine("Reinvestment Offset: {0} days", reinvestmentOffset);
            Console.WriteLine("Reinvestment Amount: ${0:N2}", reinvestmentAmount);
            Console.WriteLine("Initial Principle (PV): ${0:N2}", initialPrinciple);
            Console.WriteLine("-----------------------------------");
            var total = Calc.Calc(days, reinvestmentPeriod, reinvestmentAmount, reinvestmentOffset, initialPrinciple);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Total: ${0:N2}", total);
        }
    }
}