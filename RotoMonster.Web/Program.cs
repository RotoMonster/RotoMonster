using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace RotoMonster
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .Build();

            string sport = configuration["Sport"].ToUpper();
            string logConnectionString = configuration
                .GetConnectionString("RotoMonsterDb")
                .Replace("{sport}", sport);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    connectionString: logConnectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "_Logs",
                        AutoCreateSqlTable = true
                    })
                .CreateLogger();

            PruneLogs(logConnectionString, configuration.GetValue<int>("LogRetentionDays", 30));

            CreateHostBuilder(args).Build().Run();
        }

        private static void PruneLogs(string connectionString, int retentionDays)
        {
            if (retentionDays <= 0)
                return;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "IF OBJECT_ID('_Logs', 'U') IS NOT NULL DELETE FROM _Logs WHERE TimeStamp < DATEADD(day, @days, GETUTCDATE())";
                    command.Parameters.AddWithValue("@days", -1 * retentionDays);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Log pruning failed");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
