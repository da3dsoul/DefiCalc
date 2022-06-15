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
                if (cliArgs.InitialInvestment == 0) cliArgs.InitialInvestment = cliArgs.InitialPrinciple;
                List<InvestmentSchedule> schedules;
                if (cliArgs.SchedulePath == null)
                {
                    schedules = new List<InvestmentSchedule>
                    {
                        new()
                        {
                            Amount = cliArgs.ReinvestmentAmount, Period = cliArgs.ReinvestmentPeriod,
                            StartDate = DateTime.Today.AddDays(cliArgs.ReinvestmentOffset)
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
                            DateTime.Today.AddDays(eventArgs.Day), eventArgs.InterestRate, eventArgs.AmountToAdd,
                            eventArgs.AmountPendingExtraction,
                            eventArgs.AmountExtracted, eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                    else
                    {
                        if (schedules.Count > 0)
                            Console.WriteLine(
                                "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Additional Investment: ${3:N2} | Subtotal: ${4:N2} | Taxes: ${5:N2} | Fees: ${6:N2} | Total: ${7:N2}",
                                DateTime.Today.AddDays(eventArgs.Day),
                                eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.AdditionalInvestment - eventArgs.AmountWithdrawn,
                                eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                        else
                            Console.WriteLine(
                                "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Subtotal: ${3:N2} | Taxes: ${4:N2} | Fees: ${5:N2} | Total: ${6:N2}",
                                DateTime.Today.AddDays(eventArgs.Day),
                                eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                    }

                    last = eventArgs;
                };

                Calc.DateChanged += (_, date) =>
                {
                    var c = CultureInfo.CurrentCulture.Calendar;
                    if (date == DateTime.Today) return;
                    var days = c.GetDaysInMonth(date.Year, date.Month);
                    if (date.Day == DateTime.Today.Day || (DateTime.Today.Day > days && date.Day == days))
                        Console.WriteLine("------------{0:yyyy-MM-dd}-------------", date);
                };

                Console.WriteLine("-----------Settings----------------");
                Console.WriteLine("Days: {0}", cliArgs.Days);
                Console.WriteLine("Initial Principle (PV): ${0:N2}", cliArgs.InitialPrinciple);
                if (cliArgs.InitialInvestment != cliArgs.InitialPrinciple)
                    Console.WriteLine("Initial Investment: ${0:N2}", cliArgs.InitialInvestment);
                if (schedules.Count > 0)
                    Console.WriteLine("-----------Schedule----------------");
                else
                    Console.WriteLine("-----------------------------------");
                foreach (var investmentSchedule in schedules)
                {
                    if (investmentSchedule.Amount > 0)
                        Console.WriteLine("  Amount: ${0:N2}", investmentSchedule.Amount);
                    if (investmentSchedule.WithdrawAmount > 0)
                        Console.WriteLine("  Withdraw Amount: ${0:N2}", investmentSchedule.WithdrawAmount);
                    Console.WriteLine("  Start Date: {0:yyyy-MM-dd}", investmentSchedule.StartDate);
                    if (investmentSchedule.Period != 0)
                        Console.WriteLine("  Period: {0} days", investmentSchedule.Period);
                    if (investmentSchedule.EndDate != null)
                        Console.WriteLine("  End Date: {0:yyyy-MM-dd}", investmentSchedule.EndDate);
                    Console.WriteLine("-----------------------------------");
                }

                Calc.Calc(cliArgs.Days, cliArgs.InitialPrinciple, cliArgs.InitialInvestment, schedules);
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Subtotal:      ${0:N2}", last.Subtotal);
                Console.WriteLine("Tax:           ${0:N2}", last.Taxes);
                Console.WriteLine("Coinbase Fees: ${0:N2}", last.Fees);
                Console.WriteLine("Total:         ${0:N2}", last.Total);
            });
        }
    }
}