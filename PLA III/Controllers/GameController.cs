using Microsoft.AspNetCore.Mvc;
using PLA_III.Services; 
using PLA_III.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using PLA_III.DataTransferObjects; // (Asegúrate de tener este using para 'Task')

// Define la base para todos los endpoints en este controlador
[Route("api/game/v1")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameService gameService, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }


    // 1. Endpoint: POST api/game/v1/register
    //      Registra un jugador
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterPlayerRequest request)
    {
        
        _logger.LogInformation("Inicio de solicitud POST /api/game/v1/register para {FirstName}", request.FirstName);

        // Valida si la petición automática gracias a [ApiController]
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); //StatusCode 400 si el DTO no es válido
        }

        try
        {
            var response = await _gameService.RegisterPlayer(request);

            // Validamos la respuesta: 0 = el usuario ya existe (error de negocio)
            if (response.PlayerId == 0)
            {
               
                _logger.LogWarning("Registro fallido para {FirstName}: El usuario ya existe.", request.FirstName);
                return BadRequest(new { Message = "Error 400: El usuario ya existe" }); //StatusCode 400
            }

           
            _logger.LogInformation("Registro exitoso. PlayerID: {PlayerId}", response.PlayerId);
            return Ok(response); //StatusCode 200 con el Id del jugador
        }
        catch (Exception ex)
        {
           
            _logger.LogError(ex, "Error interno en /api/game/v1/register. Mensaje: {ErrorMessage}", ex.Message);

            // Captura errores internos (ej: problemas de conexión a la DB)
            return StatusCode(500, new { Message = "Error interno del servidor al registrar el jugador." });
        }

    }


    // 2. Endpoint: POST api/game/v1/start
    //      Inicializa un nuevo juego para un jugador
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartGameRequest request)
    {
       
        _logger.LogInformation("Inicio de solicitud POST /api/game/v1/start para PlayerID: {PlayerId}", request.PlayerId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _gameService.StartGame(request);

            // Validamos la respuesta de negocio que viene del servicio
            if (!string.IsNullOrEmpty(response.Message)) // Si el servicio devolvió un mensaje, es un error
            {
               
                _logger.LogWarning("Inicio de juego fallido para PlayerID {PlayerId}: {ErrorMessage}", request.PlayerId, response.Message);

                // Error de negocio (jugador no existe)
                if (response.Message == "Error 404: Jugador no encontrado.")
                {
                    return NotFound(new { Message = response.Message }); // StatusCode 404
                }

                // Error de negocio general (ej: juego activo)
                return BadRequest(new { Message = response.Message }); // StatusCode 400
            }

           
            _logger.LogInformation("Juego iniciado exitosamente. GameID: {GameId} para PlayerID {PlayerId}", response.GameId, response.PlayerId);
            return Ok(response); // StatusCode 200 con el GameId

        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "Error interno en /api/game/v1/start. Mensaje: {ErrorMessage}", ex.Message);

            return StatusCode(500, new { Message = "Error interno del servidor con el GameId" });
        }
    }

    // 3. Endpoint: POST api/game/v1/guess
    //      Procesa un intento de adivinanza
    [HttpPost("guess")]
    public async Task<IActionResult> Guess([FromBody] GuessNumberRequest request)
    {
       
        _logger.LogInformation("Inicio de solicitud POST /api/game/v1/guess para GameID: {GameId}", request.GameId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _gameService.GuessNumber(request);

            // Manejo de errores de negocio desde el servicio
            if (response.Message.StartsWith("Error"))
            {
                
                _logger.LogWarning("Intento fallido para GameID {GameId}: {ErrorMessage}", request.GameId, response.Message);

                // Error de negocio (Juego no encontrado)
                if (response.Message.Contains("404"))
                {
                    return NotFound(new { Message = "Error 404: Juego no encontrado." });
                }

                // Error de negocio general (ej: número inválido, juego terminado)
                return BadRequest(new { Message = response.Message }); // StatusCode 400
            }

           
            _logger.LogInformation("Intento procesado para GameID {GameId}. Mensaje: {Message}", request.GameId, response.Message);

            // Si es OK (devuelve la pista o la felicitación)
            return Ok(response); //StatusCode 200 con la pista y felicitación
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "Error interno en /api/game/v1/guess. Mensaje: {ErrorMessage}", ex.Message);

            return StatusCode(500, new { Message = "Error interno del servidor al procesar el intento." });
        }
    }

} 