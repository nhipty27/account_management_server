using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManagement.Infrastructure.Core.Models
{
    public class UserRole
    {
        public int userId { get; set; }
        public int roleId { get; set; }
        public int createBy { get; set; }
        public DateTime createAt { get; set; } = DateTime.Now;

    }
}
