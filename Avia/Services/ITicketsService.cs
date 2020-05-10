using System.Threading.Tasks;

namespace Avia.Services
{
    public interface ITicketsService
    {
        Task<string> GetTicketsAsync();
    }
}