using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;
using Serilog.Events;

namespace UserService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Starting web host");

            try
            {
                MigrateDatabase();
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console())
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        private static void MigrateDatabase()
        {
            try
            {
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder
                {
                    Host = Environment.GetEnvironmentVariable("POSTGRES_HOST"),
                    Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT")),
                    Database = Environment.GetEnvironmentVariable("POSTGRES_DB"),
                    Username = Environment.GetEnvironmentVariable("POSTGRES_USER"),
                    Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"),
                    SearchPath = "userservice"
                };
                var connectionString = new NpgsqlConnection(connectionStringBuilder.ConnectionString);

                var evolve = new Evolve.Evolve(connectionString, msg => Log.Information(msg))
                {
                    Locations = new[] { "db/migrations" },
                    IsEraseDisabled = true,
                    OutOfOrder = true,
                    EnableClusterMode = true,
                    MetadataTableName = "_changelog"
                };

                evolve.Migrate();
            }
            catch (Exception ex)
            {
                Log.Error("Database migration failed.", ex);
                throw;
            }
        }
    }
}