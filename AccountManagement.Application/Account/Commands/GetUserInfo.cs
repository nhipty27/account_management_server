﻿using AccountManagement.Application.Common.Models;
using AccountManagement.Application.Redis.Common;
using AccountManagement.Infrastructure.Core.Authentication;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using FluentValidation;
using Newtonsoft.Json;

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
            private readonly IRedisProvider _redisProvider;

            public Handler(IQuery query, IRedisProvider redisProvider)
            {
                _query = query;
                _redisProvider = redisProvider;
            }

            public async Task<ResultObject<UserDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                
                var result = new ResultObject<UserDto>();
                try
                {
                    //var tokenEx = _query.Query<string>("SELECT TOKEN FROM USER_TOKEN WHERE TOKEN = @token ", new {token =  request.token});
                    int userId = _query.Query<int>("SELECT userId FROM USER_TOKEN WHERE TOKEN = @token", new { token = request.token }).FirstOrDefault();
                    List<string> tokens = _redisProvider.GetList($"token{userId}");
                    if (Jwt.getClaim(request.token) == null || !_redisProvider.ExistValueByKey($"token{userId}", JsonConvert.SerializeObject(request.token)))
                    {
                        result._code = 401;
                        result.Message = "Không thể truy cập!";
                        return result;
                    }
                    var email = Jwt.getClaim(request.token);
                    var rs = _query.Query<UserDto>(@"SELECT U.*, R.name as role 
                                                    FROM USERS U, USER_ROLE UR, ROLES R
                                                    WHERE U.id = UR.userId AND UR.roleId = R.id AND U.email = @email", new { email }).FirstOrDefault();
                    
                    result.Data = rs;
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
