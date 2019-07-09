using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopshelfService
{
    public class WorkerJob : IJob
    {

        private ILog Log { get; }

        public WorkerJob(ILog log) =>
            Log = log ?? throw new ArgumentNullException(nameof(log));

        public Task Execute(IJobExecutionContext context) =>
            Task.Run(() =>
            {
                Log.Info($"Hi from WorkerJob...");
            });
    }
}
