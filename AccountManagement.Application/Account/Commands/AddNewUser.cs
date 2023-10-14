using AccountManagement.Application.Account.Dtos;
using AccountManagement.Application.Common;
using AccountManagement.Application.Common.Models;
using AccountManagement.Application.Redis.Common;
using AccountManagement.Infrastructure.Core.Authentication;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using Dapper;
using FluentValidation;
using Newtonsoft.Json;
using System.Data;

namespace AccountManagement.Application.Account.Commands
{
    public class AddNewUser
    {
        /// <summary>
		/// Script tạo tài khoản
		/// </summary>
        const string createAccount = @"
            DECLARE
                @createAt DATETIME = GETDATE(),
                @exToken DATETIME = DATEADD(DAY, @tokenDay, GETDATE()),
                @userId INT = 0,
                @roleId INT = 0,
                @ResultStatus INT = -1,
                @Message  NVARCHAR(300) = N'';
            IF ( @role <> 'USER' AND EXISTS (SELECT * 
                    FROM ROLES R, USER_ROLE UR 
                    WHERE UR.roleId = R.id AND UR.userId = @createBy AND R.name <> 'ADMIN' ) )
                BEGIN
                     SELECT @ResultStatus = -1, @Message = N'Tài khoản không thể tạo account có phân quyền người dùng!';
                     GOTO lblResult;
                END;
            --Kiểm tra tài khoản đã tồn tại
            IF EXISTS (
                SELECT * FROM USERS WHERE EMAIL = @email
            )
                BEGIN
                     SELECT @ResultStatus = -1, @Message = N'Tài khoản đã tồn tại!';
                     GOTO lblResult;
                END;

                IF NOT EXISTS (
                    SELECT * FROM ROLES WHERE NAME = @role
                )
                    BEGIN
                         SELECT @ResultStatus = -1, @Message = N'Vai trò người dùng không tồn tại!';
                         GOTO lblResult;
                    END;
            INSERT INTO USERS(email, name, password, createAt, gender, address, dayOfBirth) 
            VALUES(@email, @name, @password, @createAt, @gender, @address, @dayOfBirth);
            IF @@ROWCOUNT <= 0
                BEGIN
                    SELECT @ResultStatus = -1, @Message = N'Thêm tài khoản thất bại';
                    GOTO lblResult;
                END;
            SET @userId =  SCOPE_IDENTITY();
            SET @roleId = (SELECT ID FROM ROLES WHERE NAME = @role);
            INSERT INTO USER_ROLE(userId, roleId, createBy, createAt) VALUES(@userId, @roleId, 
                                    CASE
                                        WHEN @createBy IS NOT NULL AND @createBy > 0
                                            THEN @createBy
                                        ELSE @userId
                                    END,
                                @createAt);
            IF @@ROWCOUNT <= 0
                BEGIN
                    SELECT @ResultStatus = -1, @Message = N'Thêm tài khoản thất bại';
                    GOTO lblResult;
                END;
            IF  ISNULL(@role, '') <> ''
                BEGIN
                    INSERT INTO USER_TOKEN(userId, token, endAt, createAt) VALUES(@userId, @token, @exToken, @createAt)
                    IF @@ROWCOUNT <= 0
                        BEGIN
                            SELECT @ResultStatus = -1, @Message = N'Thêm tài khoản thất bại';
                            GOTO lblResult;
                        END
                    SET @ResultStatus = @userId;
                END
            SELECT @Message = N'Thêm tài khoản thành công';

            lblResult:
                IF @ResultStatus = -1
                BEGIN
                    DELETE FROM USER_TOKEN WHERE userId = @userId                
                    DELETE FROM USER_ROLE WHERE userId = @userId
                    DELETE FROM USERS WHERE Id = @userId
                END

                SELECT @ResultStatus AS ResultStatus, @Message AS Message, @userId as userId
        ";

        const string getRole = @"SELECT R.NAME 
                        FROM ROLES R, USER_ROLE UR
                        WHERE R.id = UR.roleId AND UR.userId = @id
                    ";

        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<UserRespone>
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
                RuleFor(x => x.Data.email.Length).GreaterThan(100).WithMessage("Chiều dài email không quá 100 ký tự");
                RuleFor(x => x.Data.email).Empty().WithMessage("Email không rỗng");

                RuleFor(x => x.Data.name).Empty().WithMessage("Họ tên không rỗng");
                RuleFor(x => x.Data.name.Length).LessThan(100).WithMessage("Họ tên không quá 100 ký tự");

                RuleFor(x => x.Data.password).Empty().WithMessage("Password không rỗng");
                RuleFor(x => x.Data.password.Length).GreaterThan(100).WithMessage("Chiều dài email tối đa 100 ký tự và tối thiểu 6 ký tự");
                RuleFor(x => x.Data.password.Length).LessThan(6).WithMessage("Chiều dài email tối đa 100 ký tự và tối thiểu 6 ký tự");
            }
        }

        public class Handler : IRequestHandlerWrapper<Command, UserRespone>
        {
            private readonly IQuery _query;
            private readonly IRedisProvider _redisProvider;
            public Handler(IQuery query, IRedisProvider redisProvider)
            {
                _query = query;
                _redisProvider = redisProvider;
            }
            
            public async Task<ResultObject<UserRespone>> Handle(Command request, CancellationToken cancellationToken)
            {
                DynamicParameters parameters = new DynamicParameters();
                var result = new ResultObject<UserRespone>();
                string jwt = Jwt.GenerateJwtToken(request.Data.email, "USER");

                //check role
                if(request.Data.role != null && request.Data.createBy > 0)
                {
                    string roleToken = Jwt.getClaim(request.Token, "role");
                    string roleId = _query.Query<string>(getRole, new { id = request.Data.createBy }).FirstOrDefault();
                    if (roleToken == roleId && roleId == "ADMIN")
                    {
                        parameters.Add("@createBy", request.Data.createBy, DbType.Int32);
                    }
                    else
                        parameters.Add("@createBy", -1, DbType.Int32);
                }
                else
                    parameters.Add("@createBy", -1 , DbType.Int32);
                parameters.Add("@email", request.Data.email ?? "", DbType.String);
                parameters.Add("@name", request.Data.name ?? "", DbType.String);
                parameters.Add("@password", HashPassword.CreatePasswordHash(request.Data.password) ?? "", DbType.String);
                parameters.Add("@role", request.Data.role ?? "USER", DbType.String);
                parameters.Add("@gender", request.Data.gender ?? null, DbType.Boolean);
                parameters.Add("@address", request.Data.address ?? null, DbType.String);
                parameters.Add("@dayOfBirth", request.Data.dayOfBirth ?? null, DbType.DateTime);
                parameters.Add("@token", jwt , DbType.String);
                parameters.Add("@tokenDay", 10, DbType.Int32);
                try
                {
                    var res = _query.Query<BaseResStore>(createAccount, parameters).FirstOrDefault();
                    if(res.ResultStatus >= 1  && request.Data.createBy <= 0)
                    {
                        result.Data = new UserRespone();
                        result.Data.token = jwt;
                        var rs = _query.Query<UserDto>(@"SELECT U.*, R.name as role 
                                                    FROM USERS U, USER_ROLE UR, ROLES R
                                                    WHERE U.id = UR.userId AND UR.roleId = R.id AND U.email = @email", new { email = request.Data.email }).FirstOrDefault();
                        result.Data.user = rs;
                        _redisProvider.PushTopIfNotExist($"token{res.ResultStatus}", JsonConvert.SerializeObject(jwt));
                    }
                    if (res.ResultStatus == -1)
                    {
                        result._code = -302;
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
