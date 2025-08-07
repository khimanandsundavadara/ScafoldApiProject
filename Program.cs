
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolProject.Models.Models.School;
using SchoolProject.Service.Repository.Implementations;
using SchoolProject.Service.Repository.Interfaces;
using Serilog;
using System.Reflection;

namespace SchoolProject
{
    public class Program
    {
        public readonly IConfiguration Configuration;
        public Program(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting up the application");

                var builder = WebApplication.CreateBuilder(args);


                var connectionString = builder.Configuration.GetConnectionString("DBConnection");
                builder.Services.AddDbContext<SchoolDbContext>(options =>
                    options.UseSqlServer(connectionString));
                Log.Information("DB context registered");

                builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly(), Assembly.Load("SchoolProject.Models"));
                Log.Information("AutoMapper registered");

                builder.Services.AddScoped<IStudentRepository, StudentRepository>();
                Log.Information("StudentRepository registered");

                builder.Services.AddControllers()
                    .AddFluentValidation(fv =>
                    {
                        fv.RegisterValidatorsFromAssembly(AppDomain.CurrentDomain.GetAssemblies()
                        .SingleOrDefault(assembly => assembly.GetName().Name == typeof(Program).Assembly.GetName().Name));
                     });


                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                app.UseMiddleware<ExceptionMiddleware>();

                if (app.Environment.IsDevelopment())
                {
                    Log.Information("Development environment - enabling Swagger");
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();

                Log.Information("Application is running...");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start correctly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

    }
}
