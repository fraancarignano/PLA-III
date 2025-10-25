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
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions =>
                    {
                        // Esta línea le dice a EF Core: "Cuando crees la base de datos, 
                        // asegúrate de usar el nombre del catálogo 'PLA_III_FINAL_DB'
                        // y no el 'master' de la cadena de conexión."
                        // Esto resuelve la ambigüedad.
                        sqlServerOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                        // Importante: No podemos forzar directamente el catálogo aquí. 
                        // La estrategia es asegurarnos de que el nombre del catálogo en 
                        // appsettings.json sea único y forzar su creación. 
                        // Volveremos a la configuración de appsettings.json.
                    }
                )
            );

            // B. Registro de Servicios
            builder.Services.AddScoped<IGameService, GameService>();

            // C. Configuración MVC y Swagger
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
