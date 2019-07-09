using log4net;
using Quartz;
using System;
using System.Threading.Tasks;

namespace TopshelfService
{
    public class WorkerService
    {

        private ILog Log { get; }
        private IScheduler Scheduler { get; }

        public WorkerService(IScheduler scheduler,
                           ILog log)
        {
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public bool OnStart()
        {
            Log.Info("Service Started.");

            // trigger async evaluation
            RunTaskAsync().GetAwaiter().GetResult();

            return true;
        }

        private async Task RunTaskAsync()
        {
            try
            {
                // and start it off
                await Scheduler.Start();

                // define the job and tie it to our WorkerJob class
                IJobDetail job = JobBuilder.Create<WorkerJob>()
                    .WithIdentity("job_worker", "group_worker")
                    .UsingJobData("jobTaskId", 5555)
                    .Build();

                // Trigger the job to run now
                ITrigger trigger = TriggerBuilder.Create()
                            .WithCronSchedule("0/5 * * ? * *", x => x
                                .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")))
                            .ForJob(job)
                            .Build();

                // Tell quartz to schedule the job using our trigger
                await Scheduler.ScheduleJob(job, trigger);
            }
            catch (SchedulerException ex)
            {
                await Console.Error.WriteLineAsync(ex.ToString());
                Log.Error(ex.ToString());
            }
        }

        public bool OnStop()
        {
            Log.Info("Service Stoped.");

            Scheduler.Shutdown();

            return true;
        }

    }
}
