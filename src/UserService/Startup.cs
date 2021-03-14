using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Prometheus;
using Serilog;

namespace UserService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks().ForwardToPrometheus();

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = Environment.GetEnvironmentVariable("POSTGRES_HOST"),
                Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT")),
                Database = Environment.GetEnvironmentVariable("POSTGRES_DB"),
                Username = Environment.GetEnvironmentVariable("POSTGRES_USER"),
                Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"),
                SearchPath = "userservice"
            };
            services.AddScoped(_ => new NpgsqlConnection(connectionStringBuilder.ConnectionString));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHealthChecks("/health");

            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseHttpMetrics();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}