using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using DefiCalc.Core.Models;
using Calc = DefiCalc.Core.Main;

namespace DefiCalc.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var parser = new Parser( o =>
                {
                    o.CaseSensitive = false;
                    o.CaseInsensitiveEnumValues = true;
                });
            var result = parser.ParseArguments<CLIArgs>(args);
            result.WithParsed(cliArgs =>
            {
                var startDate = cliArgs.StartDate ?? DateTime.Today;
                if (cliArgs.InitialInvestment == 0) cliArgs.InitialInvestment = cliArgs.InitialPrinciple;
                ScheduleModel model;
                if (cliArgs.SchedulePath == null)
                {
                    model = new ScheduleModel
                    {
                        Schedules = new InvestmentSchedule[]
                        {
                            new()
                            {
                                Amount = cliArgs.ReinvestmentAmount, Period = cliArgs.ReinvestmentPeriod,
                                StartDate = startDate.AddDays(cliArgs.ReinvestmentOffset)
                            }
                        },
                        Pledges = Array.Empty<DateRange>()
                    };
                }
                else
                {
                    var opt = new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        PropertyNameCaseInsensitive = true,
                    };
                    model = JsonSerializer.Deserialize<ScheduleModel>(File.ReadAllText(cliArgs.SchedulePath), opt) ?? new ScheduleModel
                    {
                        Schedules = Array.Empty<InvestmentSchedule>(),
                        Pledges = Array.Empty<DateRange>()
                    };
                }

                DayCalculatedEventArgs last = null;

                Calc.DayCalculated += (_, eventArgs) =>
                {
                    if (cliArgs.InitialPrinciple < 500)
                    {
                        var format = cliArgs.OutputFormat switch
                        {
                            OutputFormat.Log =>
                                "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Pending Extraction: ${3:N2} | Amount Extracted: ${4:N2} | Subtotal: ${5:N2} | Taxes: ${6:N2} | Fees: ${7:N2} | Total: ${8:N2}",
                            OutputFormat.Csv =>
                                "{0:yyyy-MM-dd},{1:P1},\"${2:N2}\",\"${3:N2}\",\"${4:N2}\",\"${5:N2}\",\"${6:N2}\",\"${7:N2}\",\"${8:N2}\"",
                            OutputFormat.Table =>
                                "|{0:yyyy-MM-dd}|{1:P1}|${2:N2}|${3:N2}|${4:N2}|${5:N2}|${6:N2}|${7:N2}|${8:N2}|",
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        Console.WriteLine(format, startDate.AddDays(eventArgs.Day), eventArgs.InterestRate,
                            eventArgs.AmountToAdd, eventArgs.AmountPendingExtraction, eventArgs.AmountExtracted,
                            eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                    }
                    else
                    {
                        if (model.Schedules.Length > 0)
                        {
                            var addInv = eventArgs.AdditionalInvestment - eventArgs.AmountWithdrawn;
                            if (addInv < 0)
                            {
                                var format = cliArgs.OutputFormat switch
                                {
                                    OutputFormat.Log =>
                                        "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Extracted: ${3:N2} | Withdrawn: ${4:N2} | Subtotal: ${5:N2} | Taxes: ${6:N2} | Fees: ${7:N2} | Total: ${8:N2}",
                                    OutputFormat.Csv =>
                                        "{0:yyyy-MM-dd},{1:P1},\"${2:N2}\",\"-${3:N2}\",\"${4:N2}\",\"${5:N2}\",\"${6:N2}\",\"${7:N2}\"",
                                    OutputFormat.Table =>
                                        "|{0:yyyy-MM-dd}|{1:P1}|${2:N2}|-${3:N2}|${4:N2}|${5:N2}|${6:N2}|${7:N2}|",
                                    _ => throw new ArgumentOutOfRangeException()
                                };
                                Console.WriteLine(
                                    format,
                                    startDate.AddDays(eventArgs.Day),
                                    eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.AmountExtracted, Math.Abs(addInv),
                                    eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                            }
                            else
                            {
                                var format = cliArgs.OutputFormat switch
                                {
                                    OutputFormat.Log =>
                                        "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Extracted: ${3:N2} | Additional Invested: ${4:N2} | Subtotal: ${5:N2} | Taxes: ${6:N2} | Fees: ${7:N2} | Total: ${8:N2}",
                                    OutputFormat.Csv =>
                                        "{0:yyyy-MM-dd},{1:P1},\"${2:N2}\",\"${3:N2}\",\"${4:N2}\",\"${5:N2}\",\"${6:N2}\",\"${7:N2}\"",
                                    OutputFormat.Table =>
                                        "|{0:yyyy-MM-dd}|{1:P1}|${2:N2}|${3:N2}|${4:N2}|${5:N2}|${6:N2}|${7:N2}|",
                                    _ => throw new ArgumentOutOfRangeException()
                                };
                                Console.WriteLine(
                                    format,
                                    startDate.AddDays(eventArgs.Day),
                                    eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.AmountExtracted, addInv,
                                    eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                            }
                        }
                        else
                        {
                            var format = cliArgs.OutputFormat switch
                            {
                                OutputFormat.Log =>
                                    "Date: {0:yyyy-MM-dd} | Interest: {1:P1} | PV*I: ${2:N2} | Subtotal: ${3:N2} | Taxes: ${4:N2} | Fees: ${5:N2} | Total: ${6:N2}",
                                OutputFormat.Csv =>
                                    "{0:yyyy-MM-dd},{1:P1},\"${2:N2}\",\"${3:N2}\",\"${4:N2}\",\"${5:N2}\",\"${6:N2}\"",
                                OutputFormat.Table =>
                                    "|{0:yyyy-MM-dd}|{1:P1}|${2:N2}|${3:N2}|${4:N2}|${5:N2}|${6:N2}|",
                                _ => throw new ArgumentOutOfRangeException()
                            };
                            Console.WriteLine(
                                format,
                                startDate.AddDays(eventArgs.Day),
                                eventArgs.InterestRate, eventArgs.AmountToAdd, eventArgs.Subtotal, eventArgs.Taxes, eventArgs.Fees, eventArgs.Total);
                        }
                    }

                    last = eventArgs;
                };

                Calc.DateChanged += (_, date) =>
                {
                    var c = CultureInfo.CurrentCulture.Calendar;
                    if (date == startDate) return;
                    if (cliArgs.OutputFormat != OutputFormat.Log) return;
                    var days = c.GetDaysInMonth(date.Year, date.Month);
                    if (date.Day == startDate.Day || (startDate.Day > days && date.Day == days))
                        Console.WriteLine("------------{0:yyyy-MM-dd}-------------", date);
                };
                switch (cliArgs.OutputFormat)
                {
                    case OutputFormat.Log:
                        WriteHeaderLog(cliArgs, startDate, model);
                        Calc.Calc(startDate, cliArgs.Days, cliArgs.InitialPrinciple, cliArgs.InitialInvestment, model);
                        WriteFooterLog(last);        
                        break;
                    case OutputFormat.Csv:
                        WriteHeaderCsv(cliArgs, model);
                        Calc.Calc(startDate, cliArgs.Days, cliArgs.InitialPrinciple, cliArgs.InitialInvestment, model);
                        break;
                    case OutputFormat.Table:
                        WriteHeaderTable(cliArgs, model);
                        Calc.Calc(startDate, cliArgs.Days, cliArgs.InitialPrinciple, cliArgs.InitialInvestment, model);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private static void WriteHeaderCsv(CLIArgs cliArgs, ScheduleModel model)
        {
            if (cliArgs.InitialPrinciple < 500)
                Console.WriteLine(
                    "Date,Interest,\"PV*I\",\"Pending Extraction\",\"Amount Extracted\",Subtotal,Taxes,Fees,Total");
            else
                Console.WriteLine(model.Schedules.Length > 0
                    ? "Date,Interest,\"PV*I\",\"Additional Invested\",Subtotal,Taxes,Fees,Total"
                    : "Date,Interest,\"PV*I\",Subtotal,Taxes,Fees,Total");
        }

        private static void WriteHeaderTable(CLIArgs cliArgs, ScheduleModel model)
        {
            if (cliArgs.InitialPrinciple < 500)
            {
                Console.WriteLine("|Date|Interest|PV*I|Pending Extraction|Amount Extracted|Subtotal|Taxes|Fees|Total|");
                Console.WriteLine("|----|--------|----|------------------|----------------|--------|-----|----|-----|");
            }
            else
            {
                if (model.Schedules.Length > 0)
                {
                    Console.WriteLine("|Date|Interest|PV*I|Additional Invested|Subtotal|Taxes|Fees|Total|");
                    Console.WriteLine("|----|--------|----|-------------------|--------|-----|----|-----|");
                }
                else
                {
                    Console.WriteLine("|Date|Interest|PV*I|Subtotal|Taxes|Fees|Total|");
                    Console.WriteLine("|----|--------|----|--------|-----|----|-----|");
                }
            }
        }

        private static void WriteHeaderLog(CLIArgs cliArgs, DateTime startDate, ScheduleModel model)
        {
            Console.WriteLine("-----------Settings----------------");
            Console.WriteLine("Start Date: {0:yyyy-MM-dd}", startDate);
            Console.WriteLine("Days: {0}", cliArgs.Days);
            Console.WriteLine("Initial Principle (PV): ${0:N2}", cliArgs.InitialPrinciple);
            if (Math.Abs(cliArgs.InitialInvestment - cliArgs.InitialPrinciple) > 0.001)
                Console.WriteLine("Initial Investment: ${0:N2}", cliArgs.InitialInvestment);
            Console.WriteLine(model.Schedules.Length > 0
                ? "-----------Schedules---------------"
                : model.Pledges.Length > 0
                    ? "-----------Pledges-----------------"
                    : "-----------------------------------");
            for (var i = 0; i < model.Schedules.Length; i++)
            {
                var investmentSchedule = model.Schedules[i];
                if (investmentSchedule.Amount > 0 || investmentSchedule.WithdrawAmount == 0)
                    Console.WriteLine("  Amount: ${0:N2}", investmentSchedule.Amount);
                if (investmentSchedule.WithdrawAmount > 0)
                    Console.WriteLine("  Withdraw Amount: ${0:N2}", investmentSchedule.WithdrawAmount);
                Console.WriteLine(
                    investmentSchedule.Period == 0 ? "  Date: {0:yyyy-MM-dd}" : "  Start Date: {0:yyyy-MM-dd}",
                    investmentSchedule.StartDate);
                if (investmentSchedule.Period != 0)
                    Console.WriteLine("  Period: {0} days", investmentSchedule.Period);
                if (investmentSchedule.EndDate != null)
                    Console.WriteLine("  End Date: {0:yyyy-MM-dd}", investmentSchedule.EndDate);
                Console.WriteLine(i == model.Schedules.Length - 1 && model.Pledges.Length > 0
                    ? "-----------Pledges-----------------"
                    : "-----------------------------------");
            }

            foreach (var dateRange in model.Pledges)
            {
                Console.WriteLine("  Start Date: {0:yyyy-MM-dd}", dateRange.StartDate);
                if (dateRange.EndDate != null)
                    Console.WriteLine("  End Date: {0:yyyy-MM-dd}", dateRange.EndDate);
                Console.WriteLine("-----------------------------------");
            }
        }

        private static void WriteFooterLog(DayCalculatedEventArgs last)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Subtotal:      ${0:N2}", last.Subtotal);
            Console.WriteLine("Tax:           ${0:N2}", last.Taxes);
            Console.WriteLine("Coinbase Fees: ${0:N2}", last.Fees);
            Console.WriteLine("Total:         ${0:N2}", last.Total);
        }
    }
}