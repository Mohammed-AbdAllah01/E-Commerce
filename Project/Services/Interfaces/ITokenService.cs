
using Project.Tables;

namespace Project.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(Person user, IList<string> roles);
    }

}
