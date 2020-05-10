using System.Threading.Tasks;

namespace Avia.Services
{
    public interface IHttpClientWrapper
    {
        Task<string> Get(string address);
    }
}