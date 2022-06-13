using System;
using System.Collections.Generic;
using System.Globalization;
using CommandLine;
using DefiCalc.Core;
using Calc = DefiCalc.Core.Main;

namespace DefiCalc.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var schedule = new List<InvestmentSchedule>
            {
                new()
                {
                    Amount = 1000,
                    StartDate = new DateTime(2022, 6, 14),
                    EndDate = new DateTime(2022, 6, 15)
                },
                new()
                {
                    Amount = 500,
                    StartDate = new DateTime(2022, 7, 12),
                    Period = 28
                },
                new()
                {
                    Amount = 500,
                    StartDate = new DateTime(2022, 8, 9),
                    Period = 14
                }
            };
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
                    {
                        if (schedule.Count > 0)
                            Console.WriteLine("Day: {0:D3}, Interest: {1:P1}, PV*I: ${2:N2}, Additional Investment: ${3:N2}, Total: ${4:N2}", eventArgs.Day,
                                eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.AdditionalInvestment, eventArgs.Total);
                        else
                            Console.WriteLine("Day: {0:D3}, Interest: {1:P1}, PV*I: ${2:N2}, Total: ${3:N2}", eventArgs.Day,
                                eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.Total);
                    }
                };

                Calc.DateChanged += (_, date) =>
                {
                    var c = CultureInfo.CurrentCulture.Calendar;
                    if (date == DateTime.Today) return;
                    var days = c.GetDaysInMonth(date.Year, date.Month);
                    if (date.Day == DateTime.Today.Day || (DateTime.Today.Day > days && date.Day == days))
                        Console.WriteLine("------------{0:yyyy-MM-dd}-------------", date);
                };

                Console.WriteLine("-----Initial Values----------------");
                Console.WriteLine("Days: {0}", cliArgs.Days);
                Console.WriteLine("Reinvestment Period: {0} days", cliArgs.ReinvestmentPeriod);
                Console.WriteLine("Reinvestment Offset: {0} days", cliArgs.ReinvestmentOffset);
                Console.WriteLine("Reinvestment Amount: ${0:N2}", cliArgs.ReinvestmentAmount);
                Console.WriteLine("Initial Principle (PV): ${0:N2}", cliArgs.InitialPrinciple);
                Console.WriteLine("-----------------------------------");
                var total = Calc.Calc(cliArgs.Days, cliArgs.InitialPrinciple, schedule);
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Total: ${0:N2}", total);
            });
        }
    }
}