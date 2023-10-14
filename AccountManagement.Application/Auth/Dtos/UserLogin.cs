using AccountManagement.Application.Common.Models;

namespace AccountManagement.Application.Auth.Dtos
{
    public class UserLogin
    {
        public string email { get; set; }
        public string password { get; set; }
    }

}
