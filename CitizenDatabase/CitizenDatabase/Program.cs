using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using CitizenDatabase.Database;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenDatabase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            InitDb(host);
            host.Run();
        }

        private static void InitDb(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
#if DEBUG
            var context = services.GetRequiredService<Database.CitizenDatabase>();
            context.Initialize();
#else
            try
            {
                var context = services.GetRequiredService<CitizenDatabase>();
                context.Initialize();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
                throw;
            }
#endif
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
