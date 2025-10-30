using Microsoft.AspNetCore.Mvc;
using PLA_III.Services; 
using PLA_III.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using PLA_III.DataTransferObjects; 


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


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterPlayerRequest request)
    {
        
        _logger.LogInformation("Inicio de solicitud POST /api/game/v1/register para {FirstName}", request.FirstName);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); 
        }

        try
        {
            var response = await _gameService.RegisterPlayer(request);

            if (response.PlayerId == 0)
            {
               
                _logger.LogWarning("Registro fallido para {FirstName}: El usuario ya existe.", request.FirstName);
                return BadRequest(new { Message = "Error 400: El usuario ya existe" }); 
            }

           
            _logger.LogInformation("Registro exitoso. PlayerID: {PlayerId}", response.PlayerId);
            return Ok(response); 
        }
        catch (Exception ex)
        {
           
            _logger.LogError(ex, "Error interno en /api/game/v1/register. Mensaje: {ErrorMessage}", ex.Message);

            return StatusCode(500, new { Message = "Error interno del servidor al registrar el jugador." });
        }

    }


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
                        
            if (!string.IsNullOrEmpty(response.Message) && response.GameId == 0)
            {
                
                if (response.Message.Contains("NotFound"))
                {
                     _logger.LogWarning("Inicio de juego fallido para PlayerID {PlayerId}: {ErrorMessage}", request.PlayerId, response.Message);

                    return NotFound(new { Message = "Error 404: Jugador no encontrado." });
                }

                
                return BadRequest(new { response.Message }); 
            }

            _logger.LogInformation("Juego iniciado exitosamente. GameID: {GameId} para PlayerID {PlayerId}", response.GameId, response.PlayerId);

            return Ok(response); 
        }
        catch (Exception)
        {
            _logger.LogError("Error interno en /api/game/v1/start. Mensaje: {ErrorMessage}");

            return StatusCode(500, new { Message = "Error interno del servidor al iniciar el juego." });
        }
    }


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

            if (response.Message.StartsWith("Error"))
            {
                
                _logger.LogWarning("Intento fallido para GameID {GameId}: {ErrorMessage}", request.GameId, response.Message);

                if (response.Message.Contains("404"))
                {
                    return NotFound(new { Message = "Error 404: Juego no encontrado." });
                }

                return BadRequest(new { Message = response.Message }); 
            }

           
            _logger.LogInformation("Intento procesado para GameID {GameId}. Mensaje: {Message}", request.GameId, response.Message);
            
            return Ok(response); 
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "Error interno en /api/game/v1/guess. Mensaje: {ErrorMessage}", ex.Message);

            return StatusCode(500, new { Message = "Error interno del servidor al procesar el intento." });
        }
    }

} 