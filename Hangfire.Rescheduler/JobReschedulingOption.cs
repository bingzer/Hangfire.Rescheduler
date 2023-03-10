namespace Hangfire.Rescheduler
{
    public class JobReschedulingOption
    {
        /// <summary>
        /// Schedules. This will override the runtime registration defined in Jobs.Registrty module
        /// </summary>
        public List<JobSchedule> Schedules { get; set; } = new List<JobSchedule>();
    }
}