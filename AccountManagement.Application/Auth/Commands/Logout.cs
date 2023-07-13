using AccountManagement.Application.Redis.Common;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using FluentValidation;
using Newtonsoft.Json;

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
                    int userId = _query.Query<int>("SELECT userId FROM USER_TOKEN WHERE TOKEN = @token", new { token = request.token }).FirstOrDefault();
                    _query.Query<string>(sqlRole, new { token = request.token });
                    _redisProvider.RemoveValueByKey($"token{userId}", JsonConvert.SerializeObject(request.token));
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
