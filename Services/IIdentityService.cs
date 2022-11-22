using Backend.Model;
using System.Threading.Tasks;

namespace Backend.Services
{
    public interface IIdentityService
    {
        Task<string> IssueJwtTokenAsync(string username);
        Task<bool> IsPasswordCorrectAsync(string username, string password);
        Task<UserModel> GetUserAsync(string username);
    }
}