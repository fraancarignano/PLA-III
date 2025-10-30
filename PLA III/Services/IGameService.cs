using PLA_III.DataTransferObjects;
using System.Threading.Tasks;

namespace PLA_III.Services
{
    public interface IGameService
    {
        Task<RegisterPlayerResponse> RegisterPlayer(RegisterPlayerRequest request);
        Task<StartGameResponse> StartGame(StartGameRequest request);
        Task<GuessNumberResponse> GuessNumber(GuessNumberRequest request);
    }
}
