using DotNetEnv;
using Server.Services;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string envPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                ".env"
            );

            if (!File.Exists(envPath))
            {
                throw new FileNotFoundException(
                    $"The .env file was not found at: {envPath}"
                );
            }

            Env.Load(envPath);

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<GeminiService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

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
