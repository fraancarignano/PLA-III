// Services/GameService.cs

using GameCore;
using Microsoft.EntityFrameworkCore;
using PLA_III.Data;
using PLA_III.DataTransferObjects;
using PLA_III.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PLA_III.Services
{
    public class GameService : IGameService
    {
        private readonly GameDbContext _context;

        public GameService(GameDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------------------------
        // Módulo 1: Registro de Jugadores (RegisterPlayer)
        // -----------------------------------------------------------------
        public async Task<RegisterPlayerResponse> RegisterPlayer(RegisterPlayerRequest request)
        {
            var existingPlayer = await _context.Players
                .Where(p => p.FirstName == request.FirstName && p.LastName == request.LastName)
                .FirstOrDefaultAsync();

            if (existingPlayer != null)
            {
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

            return new RegisterPlayerResponse
            {
                PlayerId = newPlayer.PlayerId,
                Message = "Registro exitoso."
            };
        }

        // -----------------------------------------------------------------
        // Módulo 2: Inicio de Juego (StartGame)
        // -----------------------------------------------------------------
        public async Task<StartGameResponse> StartGame(StartGameRequest request)
        {
            var player = await _context.Players.FindAsync(request.PlayerId);
            if (player == null)
            {
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
                return new StartGameResponse
                {
                    Message = $"Error: El jugador ya tiene un juego activo (ID: {activeGame.GameId})."
                };
            }

            string secretNumber = GenerateSecretNumber();

            var newGame = new Game
            {
                PlayerId = request.PlayerId,
                SecretNumber = secretNumber, // STRING
                CreateAt = DateTime.Now,
                IsFinished = false
            };

            await _context.Games.AddAsync(newGame);
            await _context.SaveChangesAsync();

            return new StartGameResponse
            {
                GameId = newGame.GameId,
                PlayerId = newGame.PlayerId,
                CreateAt = newGame.CreateAt,
                Message = "Juego iniciado."
            };
        }

        // -----------------------------------------------------------------
        // Módulo 3: Intento de Adivinanza (GuessNumber)
        // -----------------------------------------------------------------
        public async Task<GuessNumberResponse> GuessNumber(GuessNumberRequest request)
        {
            // 1. Validación: request.AttemptedNumber es STRING y se pasa a un helper STRING
            if (!ValidateGuessNumber(request.AttemptedNumber))
            {
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
                return new GuessNumberResponse { Message = "Error: El juego no se encuentra (NotFound)." };
            }

            if (game.IsFinished)
            {
                return new GuessNumberResponse { Message = $"Error: El juego {game.GameId} ya ha finalizado." };
            }

            // 4. Lógica de Picas y Famas (Uso de GameCore)
            string numeroSecreto = game.SecretNumber;
            string numeroIntento = request.AttemptedNumber;

            var resultado = Evaluator.ValidateAttempt(numeroSecreto, numeroIntento);

            int picas = resultado.Pica;
            int famas = resultado.Fama;

            string message = $"{picas} Pica{(picas != 1 ? "s" : "")}, {famas} Fama{(famas != 1 ? "s" : "")}";

            // 5. Persistencia del Intento
            
            var newAttempt = new Attempt
            {
                GameId = game.GameId,
                AttemptedNumber = request.AttemptedNumber,
                Picas = picas,
                Famas = famas,
                AttemptDate = DateTime.Now
            };
            await _context.Attempts.AddAsync(newAttempt);

            // 6. Validación de Finalización
            if (famas == 4)
            {
                game.IsFinished = true;
                _context.Games.Update(game);
                await _context.SaveChangesAsync();

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

            if (!numberString.All(char.IsDigit) || numberString.StartsWith('0'))
                return false;

            return numberString.Distinct().Count() == 4;
        }
    }
}