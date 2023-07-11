using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManagement.Application.Redis.Dtos
{

    /// <summary>
    /// Dto cơ bản của Redis.
    /// </summary>
    public class RedisDto
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
