using PLA_III.DataTransferObjects;
using System.Threading.Tasks;

namespace PLA_III.Services
{
    public interface IGameService
    {
        /// <summary>
        /// Registra un nuevo jugador en la base de datos.
        /// </summary>
        /// <param name="request">Contiene el Nombre, Apellido y Edad del jugador.</param>
        /// <returns>El PlayerId generado o un mensaje de error.</returns>
        Task<RegisterPlayerResponse> RegisterPlayer(RegisterPlayerRequest request);

        /// <summary>
        /// Inicia un nuevo juego para un jugador registrado, generando el número secreto.
        /// </summary>
        /// <param name="request">Contiene el PlayerId del jugador.</param>
        /// <returns>El GameId generado o un mensaje de error si ya tiene un juego activo.</returns>
        Task<StartGameResponse> StartGame(StartGameRequest request);

        /// <summary>
        /// Procesa el intento del jugador y calcula las pistas (Picas y Famas).
        /// </summary>
        /// <param name="request">Contiene el GameId y el número intentado (AttemptedNumber).</param>
        /// <returns>Las pistas (Picas y Famas) o un mensaje de felicitación/error.</returns>
        Task<GuessNumberResponse> GuessNumber(GuessNumberRequest request);
    }
}
