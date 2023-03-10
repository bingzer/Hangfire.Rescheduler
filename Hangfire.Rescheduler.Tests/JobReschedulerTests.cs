using Hangfire.Storage;
using Moq;

namespace Hangfire.Rescheduler.Tests
{
    [TestClass]
    public class JobSchedulerTests
    {
        [TestMethod]
        public void IsInstanceOf_IJobScheduler()
        {
            var storage = new Mock<JobStorage>();

            var jobManager = new Mock<IRecurringJobManager>();

            var scheduler = new JobRescheduler(storage.Object, jobManager.Object);

            Assert.IsInstanceOfType(scheduler, typeof(IJobRescheduler));
        }

        [TestMethod]
        public void LoadSchedules()
        {
            var jobSchedule = new JobSchedule
            {
                JobId = "DirectoryCleaner",
                Cron = "* * * * 0",
                IsEnabled = true,
                TimeZoneId = TimeZoneInfo.Utc.Id
            };

            var options = new JobReschedulingOption
            {
                Schedules = new List<JobSchedule>
                {
                    jobSchedule
                }
            };

            var storageConnection = new Mock<IStorageConnection>();

            var storage = new Mock<JobStorage>();
            storage.Setup(c => c.GetConnection()).Returns(storageConnection.Object)
                .Verifiable();

            var jobManager = new Mock<IRecurringJobManager>();
            jobManager.Setup(c => c.AddOrUpdate(It.IsAny<string>(),
                It.IsAny<Hangfire.Common.Job>(),
                It.IsAny<string>(),
                It.IsAny<RecurringJobOptions>()))
                .Verifiable();

            var scheduler = new JobRescheduler(storage.Object, jobManager.Object);

            scheduler.LoadSchedules(options);

            jobManager.VerifyAll();
            storage.VerifyAll();
            storageConnection.VerifyAll();
        }

        [TestMethod]
        public void LoadSchedules_EnabledFalse_Remove()
        {
            var jobSchedule = new JobSchedule
            {
                JobId = "DirectoryCleaner",
                Cron = "* * * * 0",
                IsEnabled = false,
                TimeZoneId = TimeZoneInfo.Utc.Id
            };

            var options = new JobReschedulingOption
            {
                Schedules = new List<JobSchedule>
                {
                    jobSchedule
                }
            };

            var storageConnection = new Mock<IStorageConnection>();

            var storage = new Mock<JobStorage>();
            storage.Setup(c => c.GetConnection()).Returns(storageConnection.Object)
                .Verifiable();

            var jobManager = new Mock<IRecurringJobManager>();
            jobManager.Setup(c => c.RemoveIfExists("DirectoryCleaner")).Verifiable();

            var scheduler = new JobRescheduler(storage.Object, jobManager.Object);

            scheduler.LoadSchedules(options);

            jobManager.VerifyAll();
            storage.VerifyAll();
            storageConnection.VerifyAll();
        }
    }
}