using AccountManagement.Application.Auth.Dtos;
using AccountManagement.Application.Redis.Common;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using FluentValidation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManagement.Application.Auth.Commands
{
    public class Test
    {
        public class Command : IRequestWrapper<string>
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }

        /// <summary>
        /// Validate
        /// </summary>
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
            }
        }

        public class Handler : IRequestHandlerWrapper<Command, string>
        {
            public async Task<ResultObject<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = new ResultObject<string>();

                UserLogin A = new UserLogin()
                {
                    email = "nhiphan99@gmail.com",
                    password = "password"
                };

                string col = "email";
                System.Reflection.PropertyInfo pi = A.GetType().GetProperty(col);
                string val = (String)(pi.GetValue(A, null));
                return result;
            }
        }
    }
}
