namespace DefiCalc.Core.Models
{
    public class ScheduleModel
    {
        public InvestmentSchedule[] Schedules { get; set; }
        public DateRange[] Pledges { get; set; }
    }
}