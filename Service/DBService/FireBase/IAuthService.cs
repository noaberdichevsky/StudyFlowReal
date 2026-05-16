using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyFlow.Service.DBService.FireBase
{
    public interface IAuthService
    {
        Task<string> SignIn(string userEmail, string userPassword);
        Task<string> CreateAuth(string email, string password);
        Task RemoveAuth(string email, string password);
        Task SignOut();
    }
}
