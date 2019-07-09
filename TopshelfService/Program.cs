using Autofac;
using Autofac.Extras.Quartz;
using Autofac.log4net;
using log4net.Config;
using Quartz;
using System.Collections.Specialized;
using System.Configuration;
using Topshelf;
using Topshelf.Autofac;

namespace TopshelfService
{
    class Program
    {
        static void Main(string[] args)
        {
            // init log4net
            XmlConfigurator.Configure();

            // create container
            var builder = new ContainerBuilder();

            builder.RegisterModule(new QuartzAutofacFactoryModule
            {
                ConfigurationProvider = context =>
                    (NameValueCollection)ConfigurationManager.GetSection("quartz")
            });

            builder.RegisterModule<Log4NetModule>();
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(WorkerService).Assembly));
            builder.RegisterType<WorkerService>();
            builder.RegisterType<WorkerJob>().As<IJob>();

            var container = builder.Build();

            // create host factory
            HostFactory.Run(config =>
            {
                //config service
                config.SetServiceName("AWorkerService");
                config.SetDisplayName("A Worker Service");
                config.SetDescription("Worker service");
                config.RunAsLocalSystem();

                // use log4net on host factory
                config.UseLog4Net();

                // pass container to Topshelf
                config.UseAutofacContainer(container);

                //config worker service
                config.Service<WorkerService>(task =>
                {
                    // Topshelf use autofac container
                    task.ConstructUsingAutofacContainer();
                    task.WhenStarted((service, control) => service.OnStart());
                    task.WhenStopped((service, control) => service.OnStop());
                });
            });
        }
    }
}
