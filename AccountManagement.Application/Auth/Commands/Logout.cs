using AccountManagement.Application.Redis.Common;
using AccountManagement.Infrastructure.Core.Authentication;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using FluentValidation;

namespace AccountManagement.Application.Auth.Commands
{
    public class Logout
    {
        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<string>
        {
            public string token { get; set; }
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

                try {
                    string sqlRole = @"DELETE FROM USER_TOKEN WHERE TOKEN = @token";
                    _query.Query<string>(sqlRole, new { token = request.token });
                    var email = JwtEventsHandler.getClaim(request.token);
                    _redisProvider.DeleteByKey($"token {email}");
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
