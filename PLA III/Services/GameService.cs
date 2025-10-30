using GameCore;
using Microsoft.EntityFrameworkCore;
using PLA_III.Data;
using PLA_III.DataTransferObjects;
using PLA_III.Models;

namespace PLA_III.Services
{
    public class GameService : IGameService
    {
        private readonly GameDbContext _context;
        private readonly ILogger<GameService> _logger; 

       
        public GameService(GameDbContext context, ILogger<GameService> logger)
        {
            _context = context;
            _logger = logger; 
        }

        // Registro de Jugadores (RegisterPlayer)
        public async Task<RegisterPlayerResponse> RegisterPlayer(RegisterPlayerRequest request)
        {
            var existingPlayer = await _context.Players
                .Where(p => p.FirstName == request.FirstName && p.LastName == request.LastName)
                .FirstOrDefaultAsync();

            if (existingPlayer != null)
            {
                
                _logger.LogWarning("AUDITORÍA: Intento de registro fallido. El usuario {FirstName} {LastName} ya existe.", request.FirstName, request.LastName);

                return new RegisterPlayerResponse
                {
                    PlayerId = 0,
                    Message = "Error: El usuario ya se encuentra registrado."
                };
            }

            var newPlayer = new Player
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Age = request.Age,
                RegistrationDate = DateTime.Now
            };

            await _context.Players.AddAsync(newPlayer);
            await _context.SaveChangesAsync();

            
            _logger.LogInformation(
                "AUDITORÍA: Nuevo jugador registrado. PlayerID: {PlayerId}, Nombre: {FirstName}, Apellido: {LastName}, Fecha: {RegisterDate}",
                newPlayer.PlayerId, newPlayer.FirstName, newPlayer.LastName, newPlayer.RegistrationDate);

            return new RegisterPlayerResponse
            {
                PlayerId = newPlayer.PlayerId,
                Message = "Registro exitoso."
            };
        }

        // Inicio de Juego (StartGame)
        public async Task<StartGameResponse> StartGame(StartGameRequest request)
        {
            var player = await _context.Players.FindAsync(request.PlayerId);
            if (player == null)
            {
               
                _logger.LogWarning("AUDITORÍA: Inicio de juego fallido. PlayerID {PlayerId} no encontrado.", request.PlayerId);

                return new StartGameResponse
                {
                    Message = "Error: El jugador no se encuentra registrado (NotFound)."
                };
            }

            var activeGame = await _context.Games
                .Where(g => g.PlayerId == request.PlayerId && g.IsFinished == false)
                .FirstOrDefaultAsync();

            if (activeGame != null)
            {
               
                _logger.LogWarning("AUDITORÍA: Inicio de juego fallido. PlayerID {PlayerId} ya tiene un juego activo (GameID: {ActiveGameId}).", request.PlayerId, activeGame.GameId);

                return new StartGameResponse
                {
                    Message = $"Error: El jugador ya tiene un juego activo (ID: {activeGame.GameId})."
                };
            }

            string secretNumber = GenerateSecretNumber();

            var newGame = new Game
            {
                PlayerId = request.PlayerId,
                SecretNumber = secretNumber, 
                CreateAt = DateTime.Now,
                IsFinished = false
            };

            await _context.Games.AddAsync(newGame);
            await _context.SaveChangesAsync();

            
            _logger.LogInformation(
                "AUDITORÍA: Nuevo juego iniciado. GameID: {GameId}, PlayerID: {PlayerId}, Fecha: {CreateAt}",
                newGame.GameId, newGame.PlayerId, newGame.CreateAt);

            return new StartGameResponse
            {
                GameId = newGame.GameId,
                PlayerId = newGame.PlayerId,
                CreateAt = newGame.CreateAt,
                Message = "Juego iniciado."
            };
        }

        
        // Intento de Adivinanza (GuessNumber)
        public async Task<GuessNumberResponse> GuessNumber(GuessNumberRequest request)
        {
            if (!ValidateGuessNumber(request.AttemptedNumber))
            {
               
                _logger.LogWarning("AUDITORÍA: Intento rechazado (GameID: {GameId}). El número {AttemptedNumber} no es válido.", request.GameId, request.AttemptedNumber);

                return new GuessNumberResponse
                {
                    GameId = request.GameId,
                    AttemptedNumber = request.AttemptedNumber,
                    Message = "Error: El número de intento debe ser de 4 dígitos únicos (0000 a 9999)."
                };
            }

            var game = await _context.Games
                .Where(g => g.GameId == request.GameId)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                
                _logger.LogWarning("AUDITORÍA: Intento rechazado. GameID {GameId} no encontrado.", request.GameId);
                return new GuessNumberResponse { Message = "Error: El juego no se encuentra (NotFound)." };
            }

            if (game.IsFinished)
            {
               
                _logger.LogWarning("AUDITORÍA: Intento rechazado. El juego {GameId} ya ha finalizado.", game.GameId);
                return new GuessNumberResponse { Message = $"Error: El juego {game.GameId} ya ha finalizado." };
            }

            string numeroSecreto = game.SecretNumber;
            string numeroIntento = request.AttemptedNumber;

            var resultado = Evaluator.ValidateAttempt(numeroSecreto, numeroIntento);
            int picas = resultado.Pica;
            int famas = resultado.Fama;
            string message = $"{picas} Pica{(picas != 1 ? "s" : "")}, {famas} Fama{(famas != 1 ? "s" : "")}";

            var newAttempt = new Attempt
            {
                GameId = game.GameId,
                AttemptedNumber = request.AttemptedNumber,
                Picas = picas,
                Famas = famas,
                AttemptDate = DateTime.Now
            };
            await _context.Attempts.AddAsync(newAttempt);

            
            _logger.LogInformation(
                "AUDITORÍA: Nuevo intento. GameID: {GameId}, PlayerID: {PlayerId}, Intento: {AttemptedNumber}, Picas: {Picas}, Famas: {Famas}",
                game.GameId, game.PlayerId, newAttempt.AttemptedNumber, newAttempt.Picas, newAttempt.Famas);

            if (famas == 4)
            {
                game.IsFinished = true;
                _context.Games.Update(game);
                await _context.SaveChangesAsync(); 

                var totalAttempts = await _context.Attempts.CountAsync(a => a.GameId == game.GameId);

                
                _logger.LogInformation(
                    "AUDITORÍA: ¡Juego finalizado! GameID: {GameId}, PlayerID: {PlayerId} adivinó. Total de intentos: {TotalAttempts}",
                    game.GameId, game.PlayerId, totalAttempts);

                return new GuessNumberResponse
                {
                    GameId = game.GameId,
                    AttemptedNumber = request.AttemptedNumber,
                    Picas = picas,
                    Famas = famas,
                    Message = $"¡Felicidades! Has adivinado el número secreto ({game.SecretNumber})."
                };
            }
            else
            {
                await _context.SaveChangesAsync(); 

                return new GuessNumberResponse
                {
                    
                    GameId = game.GameId,
                    AttemptedNumber = request.AttemptedNumber,
                    Picas = picas,
                    Famas = famas,
                    Message = message
                };
            }
        }


        // Helpers
        private string GenerateSecretNumber()
        {
            Random random = new Random();
            var digits = Enumerable.Range(0, 10).ToList();
            var firstDigit = random.Next(1, 10);
            digits.Remove(firstDigit);
            var remainingDigits = digits.OrderBy(d => random.Next()).Take(3).ToList();
            return firstDigit.ToString() + string.Join("", remainingDigits);
        }

        private bool ValidateGuessNumber(string numberString)
        {
            if (numberString.Length != 4)
                return false;
            if (!numberString.All(char.IsDigit))
            //if (!numberString.All(char.IsDigit) || numberString.StartsWith('0'))
                return false;
            return numberString.Distinct().Count() == 4;
        }
    }
}