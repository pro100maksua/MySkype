using System.Threading.Tasks;
using MySkype.WpfClient.Models;
using RestEase;

namespace MySkype.WpfClient.ApiInterfaces
{
    public interface ILoginApi
    {
        [Post("login")]
        [AllowAnyStatusCode]
        Task<Response<string>> LoginAsync([Body] TokenRequest loginForm);

        [Post("register")]
        Task<Response<string>> RegisterAsync([Body] SignUpRequest registerForm);
    }
}