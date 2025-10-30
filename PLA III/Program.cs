using Microsoft.EntityFrameworkCore;
using PLA_III.Data;
using PLA_III.Services; 

namespace PLA_III
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Registro del DbContext
            builder.Services.AddDbContext<GameDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );

            
            builder.Services.AddScoped<IGameService, GameService>();

            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy => policy
                        .WithOrigins("http://localhost:3000", "http://localhost:5173") // Ajusta según tu puerto del front
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            var app = builder.Build();

            
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.UseCors("AllowFrontend");

            app.Run();
        }
    }
}
