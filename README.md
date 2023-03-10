# Hangfire.Rescheduler

A small library to enable/disable or reschedule Hangfire jobs using appSetting.json or other options without having to recompile your code.

Usage:

Recurring jobs:
```csharp
 RecurringJob.AddOrUpdate<DirectoryCleaner>("DirectoryCleaner", job => job.Cleanup(), Cron.Daily);
 RecurringJob.AddOrUpdate<CacheCleaner>("CacheCleaner", job => job.CleanCache(), Cron.Daily);
```

If there's a need to disable `CacheCleaner` job. You have two options:
1. Remove the line from code and recompile and deploy
2. Using Hangfire Dashboard, delete the Job.
   However upon startup, the job will get added back on (depending how you set the job registrations).

## Consider the following approach:

`appSettings.json`
```json
{
  "jobOptions": {
    "schedules": [{
        "jobId": "CacheCleaner",
        "isEnabled": true,
        "cron": "0 1 * * *"
      },{
        "jobId": "DirectoryCleaner",
        "isEnabled": true,
        "cron": "0 1 * * *",
        "timezoneId": "UTC"
      },
    ]
  }
}
```

`Program.cs`
```csharp
...
RecurringJob.AddOrUpdate<DirectoryCleaner>("DirectoryCleaner", job => job.Cleanup(), Cron.Daily);
RecurringJob.AddOrUpdate<CacheCleaner>("CacheCleaner", job => job.CleanCache(), Cron.Daily);

IServiceProvider services = ...
IConfigurationRoot root = ...

var options = root.getConfiguration<JobReschedulingOption>();
var jobRescheduler = services.GetService<IJobRescheduler>("JobOptions");
jobRescheduler.LoadSchedules(options);
...
```

The next time you need to disable or reschedule, all you need to do is modify the appSettings.json and recycle the app pool!
