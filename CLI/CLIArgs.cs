using System;
using CommandLine;

namespace DefiCalc.CLI
{
    public class CLIArgs
    {
        [Option(HelpText = "The date to start calculating from")]
        public DateTime? StartDate { get; set; }
        [Option(HelpText = "The number of days to forecast", Required = true)]
        public int Days { get; set; }

        [Option(HelpText = "How many days between additional deposits")]
        public int ReinvestmentPeriod { get; set; }

        [Option(HelpText = "How much is added at each reinvestment period")]
        public double ReinvestmentAmount { get; set; }

        [Option(HelpText = "How many days from now to start the reinvestment periods (I get paid in 3 days, for example)")]
        public int ReinvestmentOffset { get; set; }

        [Option(HelpText = "Initial Investment Amount")]
        public double InitialPrinciple { get; set; }
        
        [Option(HelpText = "Initial Investment Amount (for tax purposes, you don't pay tax on initial investment, only 'Capital Gains')")]
        public double InitialInvestment { get; set; }
        
        [Option(HelpText = "Path to a json with the schedule")]
        public string SchedulePath { get; set; }
    }
}