﻿using System;
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

                Calc.DayCalculated += (_, eventArgs) =>
                {
                    if (cliArgs.InitialPrinciple < 500)
                        Console.WriteLine(
                            "Date: {0:yyyy-MM-dd}, Interest: {1:P1}, PV*I: ${2:N2}, Pending Extraction: ${3:N2}, Amount Extracted: ${4:N2}, Total: ${5:N2}",
                            DateTime.Today.AddDays(eventArgs.Day), eventArgs.InterestRate, eventArgs.AmountToAdd,
                            eventArgs.AmountPendingExtraction,
                            eventArgs.AmountExtracted, eventArgs.Total);
                    else
                    {
                        if (schedules.Count > 0)
                            Console.WriteLine(
                                "Date: {0:yyyy-MM-dd}, Interest: {1:P1}, PV*I: ${2:N2}, Additional Investment: ${3:N2}, Total: ${4:N2}",
                                DateTime.Today.AddDays(eventArgs.Day),
                                eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.AdditionalInvestment - eventArgs.AmountWithdrawn,
                                eventArgs.Total);
                        else
                            Console.WriteLine(
                                "Date: {0:yyyy-MM-dd}, Interest: {1:P1}, PV*I: ${2:N2}, Total: ${3:N2}",
                                DateTime.Today.AddDays(eventArgs.Day),
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

                Console.WriteLine("-----------Settings----------------");
                Console.WriteLine("Days: {0}", cliArgs.Days);
                Console.WriteLine("Initial Principle (PV): ${0:N2}", cliArgs.InitialPrinciple);
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

                var total = Calc.Calc(cliArgs.Days, cliArgs.InitialPrinciple, schedules);
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Subtotal:      ${0:N2}", total);
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

                var taxes = (total - bracket) * taxRate + flatTax;
                Console.WriteLine("Tax:           ${0:N2}", taxes);
                var coinbase = total * 0.01D + 4.95D;
                Console.WriteLine("Coinbase Fees: ${0:N2}", coinbase);
                Console.WriteLine("Total:         ${0:N2}", total - taxes - coinbase);
            });
        }
    }
}