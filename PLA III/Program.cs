using Microsoft.EntityFrameworkCore;
using PLA_III.Data;
using PLA_III.Services; // Aseg�rate de que IGameService y GameService est�n en el namespace correcto

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
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions =>
                    {
                        // Esta l�nea le dice a EF Core: "Cuando crees la base de datos, 
                        // aseg�rate de usar el nombre del cat�logo 'PLA_III_FINAL_DB'
                        // y no el 'master' de la cadena de conexi�n."
                        // Esto resuelve la ambig�edad.
                        sqlServerOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                        // Importante: No podemos forzar directamente el cat�logo aqu�. 
                        // La estrategia es asegurarnos de que el nombre del cat�logo en 
                        // appsettings.json sea �nico y forzar su creaci�n. 
                        // Volveremos a la configuraci�n de appsettings.json.
                    }
                )
            );

            // B. Registro de Servicios
            builder.Services.AddScoped<IGameService, GameService>();

            // C. Configuraci�n MVC y Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


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

            app.Run();
        }
    }
}
