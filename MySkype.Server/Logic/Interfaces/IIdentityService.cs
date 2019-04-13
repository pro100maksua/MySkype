using System.Threading.Tasks;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Dto;

namespace MySkype.Server.Logic.Interfaces
{
    public interface IIdentityService
    {
        Task<string> LoginAsync(TokenRequest request);
        Task<string> RegisterAsync(RegisterRequest registerRequest);
    }
}