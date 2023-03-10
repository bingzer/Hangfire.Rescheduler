using Hangfire.Storage;

namespace Hangfire.Rescheduler
{
    public interface IJobRescheduler
    {
        /// <summary>
        /// Load from configuration and override the schdule set via code.
        /// If configuration is null, it will get from the singleton instance of JobConfiguration
        /// </summary>
        void LoadSchedules(JobReschedulingOption options);
    }

    public class JobRescheduler : IJobRescheduler
    {
        private readonly JobStorage _jobStorage;
        private readonly IRecurringJobManager _recurringJobManager;

        public JobRescheduler(JobStorage jobStorage, IRecurringJobManager recurringJobManager)
        {
            _jobStorage = jobStorage;
            _recurringJobManager = recurringJobManager;
        }

        public void LoadSchedules(JobReschedulingOption options)
        {
            var storageConnection = _jobStorage.GetConnection();

            foreach (var scheduling in options.Schedules)
            {
                var recurringJob = storageConnection.GetRecurringJobs(new List<string> { scheduling.JobId }).FirstOrDefault();
                if (recurringJob == null)
                    continue;

                if (!scheduling.IsEnabled)
                {
                    _recurringJobManager.RemoveIfExists(scheduling.JobId);
                    continue;
                }

                var timezoneId = scheduling.TimeZoneId ?? recurringJob.TimeZoneId;
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                if (timezone == null)
                    throw new Exception($"Unable to find TimeZoneInfo : {timezoneId}");

                var cron = scheduling.Cron;
                if (cron == null)
                    throw new Exception($"Cron is required");

                _recurringJobManager.AddOrUpdate(recurringJob.Id,
                    recurringJob.Job,
                    scheduling.Cron,
                    timezone);
            }
        }
    }

}