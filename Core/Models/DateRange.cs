// ReSharper disable UnusedAutoPropertyAccessor.Global
using System;

namespace DefiCalc.Core.Models
{
    public record DateRange
    {
        /// <summary>
        /// The date to start the schedule. It is assumed that there will be an investment on the first day
        /// </summary>
        public DateTime StartDate { get; init; }
        /// <summary>
        /// End Date, null to be forever. An investment is only made on the end date if it lines up with the period
        /// </summary>
        public DateTime? EndDate { get; init; }
    }
}