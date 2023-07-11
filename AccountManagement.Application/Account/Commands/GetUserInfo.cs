using AccountManagement.Application.Common.Models;
using AccountManagement.Infrastructure.Core.Authentication;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using FluentValidation;

namespace AccountManagement.Application.Account.Commands
{
    public class GetUserInfo
    {
        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<UserDto>
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

        public class Handler : IRequestHandlerWrapper<Command, UserDto>
        {
            private readonly IQuery _query;

            public Handler(IQuery query)
            {
                _query = query;
            }

            public async Task<ResultObject<UserDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                
                var result = new ResultObject<UserDto>();
                try
                {
                    if (JwtEventsHandler.getClaim(request.token) == null)
                    {
                        result._code = 400;
                        return result;
                    }
                    var email = JwtEventsHandler.getClaim(request.token);
                    var rs = _query.Query<UserDto>("SELECT * FROM USERS WHERE EMAIL = @email", new { email }).FirstOrDefault();
                    string roleSql = @"SELECT R.NAME 
                                        FROM ROLES R, USER_ROLE UR, USERS U 
                                        WHERE U.id = UR.userId AND UR.roleId = R.id AND U.email = @email";
                    var role = _query.Query<string>(roleSql, new { email }).FirstOrDefault();
                    result.Data = rs;
                    result.Data.role = role;
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
