using Microsoft.EntityFrameworkCore;
using PLA_III.Data;
using PLA_III.Services; // Asegúrate de que IGameService y GameService estén en el namespace correcto

namespace PLA_III
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // A. Registro del DbContext
            // Usamos UseSqlServer, y configuramos el nombre de la BD final
            builder.Services.AddDbContext<GameDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );

            // B. Registro de Servicios
            builder.Services.AddScoped<IGameService, GameService>();

            // C. Configuración MVC y Swagger
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

            // Configure the HTTP request pipeline.
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
