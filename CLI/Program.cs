using System;
using System.Globalization;
using CommandLine;
using Calc = DefiCalc.Core.Main;

namespace DefiCalc.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CLIArgs>(args);
            result.WithParsed(cliArgs =>
            {
                Calc.DayCalculated += (_, eventArgs) =>
                {
                    if (cliArgs.InitialPrinciple < 500)
                        Console.WriteLine(
                            "Day: {0:D3}, Interest: {1:P1}, PV*I: ${2:N2}, Pending Withdraw: ${3:N2}, Amount Withdrawn: ${4:N2}, Total: ${5:N2}",
                            eventArgs.Day, eventArgs.InterestRate, eventArgs.AmountToAdd,
                            eventArgs.AmountPendingWithdraw,
                            eventArgs.AmountWithdrawn, eventArgs.Total);
                    else
                        Console.WriteLine("Day: {0:D3}, Interest: {1:P1}, PV*I: ${2:N2}, Total: ${3:N2}", eventArgs.Day,
                            eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.Total);
                };

                Calc.DateChanged += (_, date) =>
                {
                    var c = CultureInfo.CurrentCulture.Calendar;
                    var yest = date.AddDays(-1);
                    if (yest == DateTime.Today) return;
                    var days = c.GetDaysInMonth(yest.Year, yest.Month);
                    if (yest.Day == DateTime.Today.Day || (DateTime.Today.Day > days && yest.Day == days))
                        Console.WriteLine("------------{0:yyyy-MM-dd}-------------", yest);
                };

                Console.WriteLine("-----Initial Values----------------");
                Console.WriteLine("Days: {0}", cliArgs.Days);
                Console.WriteLine("Reinvestment Period: {0} days", cliArgs.ReinvestmentPeriod);
                Console.WriteLine("Reinvestment Offset: {0} days", cliArgs.ReinvestmentOffset);
                Console.WriteLine("Reinvestment Amount: ${0:N2}", cliArgs.ReinvestmentAmount);
                Console.WriteLine("Initial Principle (PV): ${0:N2}", cliArgs.InitialPrinciple);
                Console.WriteLine("-----------------------------------");
                var total = Calc.Calc(cliArgs.Days, cliArgs.ReinvestmentPeriod, cliArgs.ReinvestmentAmount,
                    cliArgs.ReinvestmentOffset, cliArgs.InitialPrinciple);
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Total: ${0:N2}", total);
            });
        }
    }
}