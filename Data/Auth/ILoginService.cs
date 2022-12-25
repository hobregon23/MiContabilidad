using System.Threading.Tasks;

namespace MiContabilidad.Data.Auth
{
    public interface ILoginService
    {
        Task Login(string token);
        Task Logout();
    }
}
