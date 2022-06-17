using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using CommandLine;
using DefiCalc.Core;
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
                var startDate = cliArgs.StartDate ?? DateTime.Today;
                if (cliArgs.InitialInvestment == 0) cliArgs.InitialInvestment = cliArgs.InitialPrinciple;
                List<InvestmentSchedule> schedules;
                if (cliArgs.SchedulePath == null)
                {
                    schedules = new List<InvestmentSchedule>
                    {
                        new()
                        {
                            Amount = cliArgs.ReinvestmentAmount, Period = cliArgs.ReinvestmentPeriod,
                            StartDate = startDate.AddDays(cliArgs.ReinvestmentOffset)
                        }
                    };
                }
                else
                {
                    schedules = JsonSerializer.Deserialize<List<InvestmentSchedule>>(
                        File.ReadAllText(cliArgs.SchedulePath)) ?? new List<InvestmentSchedule>();
                }

                DayCalculatedEventArgs last = null;

                Calc.DayCalculated += (_, eventArgs) =>
                {
                    if (cliArgs.InitialPrinciple < 500)
                        Console.WriteLine(
                            "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Pending Extraction: ${3:N2} | Amount Extracted: ${4:N2} | Subtotal: ${5:N2} | Taxes: ${6:N2} | Fees: ${7:N2} | Total: ${8:N2}",
                            startDate.AddDays(eventArgs.Day), eventArgs.InterestRate, eventArgs.AmountToAdd,
                            eventArgs.AmountPendingExtraction,
                            eventArgs.AmountExtracted, eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                    else
                    {
                        if (schedules.Count > 0)
                        {
                            var addInv = eventArgs.AdditionalInvestment - eventArgs.AmountWithdrawn;
                            if (addInv < 0)
                                Console.WriteLine(
                                    "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Withdrawn: ${3:N2} | Subtotal: ${4:N2} | Taxes: ${5:N2} | Fees: ${6:N2} | Total: ${7:N2}",
                                    startDate.AddDays(eventArgs.Day),
                                    eventArgs.InterestRate, eventArgs.AmountToAdd, Math.Abs(addInv),
                                    eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                            else
                                Console.WriteLine(
                                    "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Additional Invested: ${3:N2} | Subtotal: ${4:N2} | Taxes: ${5:N2} | Fees: ${6:N2} | Total: ${7:N2}",
                                    startDate.AddDays(eventArgs.Day),
                                    eventArgs.InterestRate, eventArgs.AmountToAdd, addInv,
                                    eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                        }
                        else
                            Console.WriteLine(
                                "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Subtotal: ${3:N2} | Taxes: ${4:N2} | Fees: ${5:N2} | Total: ${6:N2}",
                                startDate.AddDays(eventArgs.Day),
                                eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                    }

                    last = eventArgs;
                };

                Calc.DateChanged += (_, date) =>
                {
                    var c = CultureInfo.CurrentCulture.Calendar;
                    if (date == startDate) return;
                    var days = c.GetDaysInMonth(date.Year, date.Month);
                    if (date.Day == startDate.Day || (startDate.Day > days && date.Day == days))
                        Console.WriteLine("------------{0:yyyy-MM-dd}-------------", date);
                };

                Console.WriteLine("-----------Settings----------------");
                Console.WriteLine("Start Date: {0:yyyy-MM-dd}", startDate);
                Console.WriteLine("Days: {0}", cliArgs.Days);
                Console.WriteLine("Initial Principle (PV): ${0:N2}", cliArgs.InitialPrinciple);
                if (Math.Abs(cliArgs.InitialInvestment - cliArgs.InitialPrinciple) > 0.001)
                    Console.WriteLine("Initial Investment: ${0:N2}", cliArgs.InitialInvestment);
                Console.WriteLine(schedules.Count > 0
                    ? "-----------Schedule----------------"
                    : "-----------------------------------");
                foreach (var investmentSchedule in schedules)
                {
                    if (investmentSchedule.Amount > 0 || investmentSchedule.WithdrawAmount == 0)
                        Console.WriteLine("  Amount: ${0:N2}", investmentSchedule.Amount);
                    if (investmentSchedule.WithdrawAmount > 0)
                        Console.WriteLine("  Withdraw Amount: ${0:N2}", investmentSchedule.WithdrawAmount);
                    Console.WriteLine(investmentSchedule.Period == 0 ? "  Date: {0:yyyy-MM-dd}" : "  Start Date: {0:yyyy-MM-dd}", investmentSchedule.StartDate);
                    if (investmentSchedule.Period != 0)
                        Console.WriteLine("  Period: {0} days", investmentSchedule.Period);
                    if (investmentSchedule.EndDate != null)
                        Console.WriteLine("  End Date: {0:yyyy-MM-dd}", investmentSchedule.EndDate);
                    Console.WriteLine("-----------------------------------");
                }

                Calc.Calc(startDate, cliArgs.Days, cliArgs.InitialPrinciple, cliArgs.InitialInvestment, schedules);
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Subtotal:      ${0:N2}", last.Subtotal);
                Console.WriteLine("Tax:           ${0:N2}", last.Taxes);
                Console.WriteLine("Coinbase Fees: ${0:N2}", last.Fees);
                Console.WriteLine("Total:         ${0:N2}", last.Total);
            });
        }
    }
}