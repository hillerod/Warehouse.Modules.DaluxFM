using System;
using System.IO;
using Bygdrift.Warehouse.Modules;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DaluxFM
{
    public static class TimerTrigger
    {
        [Function("TimerTrigger4")]
        public static void Run([TimerTrigger("0 0 1 * * *")] MyInfo myTimer, FunctionContext context)
        {
            var logger = context.GetLogger("Function1");
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("local.settings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();
            var importer = new Warehouse.Modules.DaluxFM.Importer(config, logger);
            importer.Run(true);
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
