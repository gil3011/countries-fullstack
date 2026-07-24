using Server.Logging;
using Server.Middleware;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Persist logs to a daily rolling file (Logs/log-yyyyMMdd.log) in addition
            // to the built-in console logger configured via appsettings.json.
            builder.Logging.AddFileLogger("Logs", LogLevel.Information, retentionDays: 30);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Wire the static logger (used by the BL/DAL layers, which are not created
            // through dependency injection) into the application logging pipeline.
            AppLogger.Init(app.Services.GetRequiredService<ILoggerFactory>());

            // Configure the HTTP request pipeline.

            // Log every request and catch any unhandled exception. Placed early so it
            // wraps the entire pipeline and records all incoming calls.
            app.UseRequestLogging();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()); app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
