// Controllers/GameController.cs

using Microsoft.AspNetCore.Mvc;
using PLA_III.DataTransferObjects;
using PLA_III.Services;
using System.Threading.Tasks;
using System;

// Define la ruta base para todos los endpoints en este controlador
[ApiController]
[Route("api/game/v1")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    
    public GameController(IGameService gameService)
    {
        
        _gameService = gameService;
    }

    // -----------------------------------------------------------------
    // 1. Endpoint: POST api/game/v1/register
    // Registro de un nuevo jugador
    // -----------------------------------------------------------------
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterPlayerRequest request)
    {
        // Validación de modelos automática gracias a [ApiController]
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // StatusCode 400 si el DTO no es válido
        }

        try
        {
            var response = await _gameService.RegisterPlayer(request);

            // El servicio devuelve PlayerId = 0 si el usuario ya existe (Error de negocio)
            if (response.PlayerId == 0)
            {
                return BadRequest(new { response.Message }); // StatusCode 400
            }

            return Ok(response); // StatusCode 200 con el ID del jugador
        }
        catch (Exception)
        {
            // Captura errores internos (ej: problemas de conexión a la DB)
            return StatusCode(500, new { Message = "Error interno del servidor al registrar el jugador." });
        }
    }

    // -----------------------------------------------------------------
    // 2. Endpoint: POST api/game/v1/start
    // Inicializa un nuevo juego para un jugador
    // -----------------------------------------------------------------
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartGameRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _gameService.StartGame(request);

            // Si el mensaje tiene contenido y GameId es 0, es un error de negocio
            if (!string.IsNullOrEmpty(response.Message) && response.GameId == 0)
            {
                // Manejo de errores de negocio desde el servicio
                if (response.Message.Contains("NotFound"))
                {
                    // Jugador no encontrado
                    return NotFound(new { Message = "Error 404: Jugador no encontrado." });
                }

                // Error de negocio general (ej: juego activo)
                return BadRequest(new { response.Message }); // StatusCode 400
            }

            return Ok(response); // StatusCode 200 con el GameId
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "Error interno del servidor al iniciar el juego." });
        }
    }

    // -----------------------------------------------------------------
    // 3. Endpoint: POST api/game/v1/guess
    // Procesa un intento de adivinanza
    // -----------------------------------------------------------------
    [HttpPost("guess")]
    public async Task<IActionResult> Guess([FromBody] GuessNumberRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _gameService.GuessNumber(request);

            if (response.Message.StartsWith("Error:"))
            {
                // Manejo de errores de negocio desde el servicio
                if (response.Message.Contains("NotFound"))
                {
                    // Juego no encontrado
                    return NotFound(new { Message = "Error 404: Juego no encontrado." });
                }

                // Error de negocio general (ej: número inválido, juego terminado)
                return BadRequest(new { response.Message }); // StatusCode 400
            }

            // Si es OK (adivinó o recibió pista)
            return Ok(response); // StatusCode 200 con la pista o felicitación
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "Error interno del servidor al procesar el intento." });
        }
    }
}