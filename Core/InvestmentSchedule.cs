using System;

namespace DefiCalc.Core
{
    public class InvestmentSchedule
    {
        /// <summary>
        /// The date to start the schedule. It is assumed that there will be an investment on the first day
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// End Date, null to be forever. An investment is only made on the end date if it lines up with the period
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// The time between investments, in days
        /// </summary>
        public int Period { get; set; }
        /// <summary>
        /// Amount to invest
        /// </summary>
        public double Amount { get; set; }
    }
}