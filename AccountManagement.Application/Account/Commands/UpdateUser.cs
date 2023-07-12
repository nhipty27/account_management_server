using AccountManagement.Application.Account.Dtos;
using AccountManagement.Application.Common.Models;
using AccountManagement.Application.Redis.Common;
using AccountManagement.Infrastructure.Core.Authentication;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using Dapper;
using FluentValidation;
using System.Data;

namespace AccountManagement.Application.Account.Commands
{
    public class UpdateUser
    {
        /// <summary>
        /// update account
        /// </summary>
        const string updateUser = @"
            DECLARE
                @ResultStatus INT = -1,                
                @roleId INT = -1,
                @Message  NVARCHAR(300) = N'';

            --update theo thông tin người dùng cung cấp
            IF @createBy > 0
                BEGIN
                    UPDATE USERS 
                        SET email = @email, name = @name, gender = @gender, address = @address, dayOfBirth = @dayOfBirth
                        WHERE id = @id
                    UPDATE USER_ROLE 
                        SET createAt = GETDATE(), createBy = @createBy, roleId = (SELECT id FROM ROLES WHERE NAME = @role)
                        WHERE userId = @id
                    IF @@ROWCOUNT <= 0
                        BEGIN
                             SELECT @ResultStatus = -1, @Message = N'Cập nhật thất bại';
                             GOTO lblResult;
                        END
                    SELECT @ResultStatus = 1, @Message = N'Cập nhật thành công';
                    GOTO lblResult;
                END
            UPDATE USERS 
            SET email = @email, name = @name, gender = @gender, address = @address, dayOfBirth = @dayOfBirth
            WHERE id = @id
            IF @@ROWCOUNT <= 0
                BEGIN
                     SELECT @ResultStatus = -1, @Message = N'Cập nhật thất bại';
                     GOTO lblResult;
                END
            SELECT @ResultStatus = 1, @Message = N'Cập nhật thành công';

            lblResult:
                SELECT @ResultStatus AS ResultStatus, @Message AS Message
        ";

        const string getRole = @"SELECT R.NAME 
                        FROM ROLES R, USER_ROLE UR
                        WHERE R.id = UR.roleId AND UR.userId = @id
                    ";

        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<bool>
        {
            public UserRequest Data { get; set; }
            public string? Token { get; set; }
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

        public class Handler : IRequestHandlerWrapper<Command, bool>
        {
            private readonly IQuery _query;
            private readonly IRedisProvider _redisProvider;

            public Handler(IQuery query, IRedisProvider redisProvider)
            {
                _query = query;
                _redisProvider = redisProvider;
            }
            public async Task<ResultObject<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = new ResultObject<bool>();
                try
                {
                    DynamicParameters parameters = new DynamicParameters();

                    if (request.Data.createBy >  0)
                    {
                        string roleToken = JwtEventsHandler.getClaim(request.Token, "role");
                        string roleId = _query.Query<string>(getRole, new { id = request.Data.createBy }).FirstOrDefault();
                        if(roleToken == roleId && roleId == "ADMIN")
                        {
                            parameters.Add("@createBy", request.Data.createBy, DbType.Int32);
                        }
                        else 
                            parameters.Add("@createBy", -1, DbType.Int32);
                    }
                    else parameters.Add("@createBy", -1, DbType.Int32);
                    parameters.Add("@role", request.Data.role ?? "", DbType.String);
                    parameters.Add("@email", request.Data.email ?? "", DbType.String);
                    parameters.Add("@name", request.Data.name ?? "", DbType.String);
                    parameters.Add("@gender", request.Data.gender, DbType.Boolean);
                    parameters.Add("@address", request.Data.address ?? null, DbType.String);
                    parameters.Add("@dayOfBirth", request.Data.dayOfBirth ?? null, DbType.DateTime);
                    parameters.Add("@id", request.Data.id, DbType.Int32);

                    var res = _query.Query<BaseResStore>(updateUser, parameters).FirstOrDefault();
                    if (res.ResultStatus == -1 )
                    {
                        result._code = -130;
                    }
                    result.Message = res.Message;
                    return result;
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
