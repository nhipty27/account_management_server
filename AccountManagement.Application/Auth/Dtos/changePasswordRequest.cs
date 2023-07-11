using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManagement.Application.Auth.Dtos
{
    public class changePasswordRequest
    {
        public int id { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public bool? isLogout { get; set; } = true;
    }
}
