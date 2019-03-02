using System.Threading.Tasks;
using MySkype.WpfClient.Models;
using RestEase;

namespace MySkype.WpfClient.ApiInterfaces
{
    public interface ILoginApi
    {
        [Post]
        [AllowAnyStatusCode]
        Task<Response<string>> LoginAsync([Body] TokenRequest loginForm);
    }
}