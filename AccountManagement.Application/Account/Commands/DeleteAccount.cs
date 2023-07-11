using AccountManagement.Application.Redis.Common;
using AccountManagement.Infrastructure.Core.Authentication;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManagement.Application.Account.Commands
{
    public class DeleteAccount
    {
        ///<summary>
        ///script delete account
        ///</summary>
        const string deleteAccount = @"
            UPDATE USER_ROLE SET createBy = null WHERE createBy = @id
            DELETE FROM USER_TOKEN WHERE userId = @id
            DELETE FROM USER_ROLE WHERE userId = @id
            DELETE FROM USERS WHERE id = @id
        ";


        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<string>
        {
            public int id { get; set; }
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
            private readonly IQuery _query;
            private readonly IRedisProvider _redisProvider;

            public Handler(IQuery query, IRedisProvider redisProvider)
            {
                _query = query;
                _redisProvider = redisProvider;
            }
            public async Task<ResultObject<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = new ResultObject<string>();

                try
                {
                    _query.Query<string>(deleteAccount, new { id = request.id });
                    _redisProvider.DeleteByKey($"token {request.id}");
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                    result._code = -130;
                }

                return result;
            }
        }
    }
}
