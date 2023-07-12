using AccountManagement.Application.Common;
using AccountManagement.Application.Common.Models;
using AccountManagement.Application.Redis.Common;
using AccountManagement.Infrastructure.Core.Authentication;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using Dapper;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace AccountManagement.Application.Auth.Commands
{
    public class Login
    {
        /// <summary>
		/// Kiểm tra thông tin đăng nhập.
		/// </summary>
        const string addToken = @"
            DECLARE
                @createAt DATETIME = GETDATE(),
                @exToken DATETIME = DATEADD(DAY, @tokenDay, GETDATE()),
                @ResultStatus INT = -1,
                @userId INT = 0,
                @Message  NVARCHAR(300) = N'';

            SET @userId  = (SELECT id FROM USERS WHERE email = @email);
            INSERT INTO USER_TOKEN(userId, token, endAt, createAt) VALUES(@userId, @token, @exToken, @createAt)
            IF @@ROWCOUNT <= 0
                BEGIN
                     SELECT @ResultStatus = -1, @Message = N'Đăng nhập thất bại';
                     GOTO lblResult;
                END
            SELECT @ResultStatus = 1, @Message = N'Đăng nhập thành công';

            lblResult:
                IF @ResultStatus = -1
                BEGIN
                    DELETE FROM USER_TOKEN WHERE userId = @userId                
                    SELECT @ResultStatus AS ResultStatus, @Message AS Message
                END
                ELSE                    
                    SELECT @userId AS ResultStatus, @Message AS Message
        ";

        ///<summary>
        ///script kiểm tra role.
        ///</summary>
        const string getRole = @"
            SELECT R.NAME 
            FROM ROLES R, USERS U, USER_ROLE UR
            WHERE U.ID = UR.USERID AND R.ID = UR.ROLEID AND U.EMAIL = @email
        ";

        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<string>
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        /// <summary>
        /// Validate
        /// </summary>
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.email.Length).GreaterThan(100).WithMessage("Chiều dài email không quá 100 ký tự");
                RuleFor(x => x.email).Empty().WithMessage("Email không rỗng");

                RuleFor(x => x.password).Empty().WithMessage("Password không rỗng");
                RuleFor(x => x.password.Length).GreaterThan(100).WithMessage("Chiều dài email tối đa 100 ký tự và tối thiểu 6 ký tự");
                RuleFor(x => x.password.Length).LessThan(6).WithMessage("Chiều dài email tối đa 100 ký tự và tối thiểu 6 ký tự");
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
                    var existUser =  _query.Query<string>("SELECT PASSWORD FROM USERS WHERE email = @email", new { email = request.email }).FirstOrDefault();

                    if (existUser.IsNullOrEmpty())
                    {
                        result.Message = "Tài khoản không tồn tại";
                        result._code = -142;
                        return result;
                    }
                    if(HashPassword.VerifyPassword(request.password, existUser))
                    {
                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@email", request.email ?? "", DbType.String);
                        parameters.Add("@name", request.password ?? "", DbType.String);
                        parameters.Add("@tokenDay", 10, DbType.Int32);

                        var role = _query.Query<string>(getRole, parameters).FirstOrDefault();
                        string jwt = JwtEventsHandler.GenerateJwtToken(request.email, role);
                        parameters.Add("@token", jwt, DbType.String);

                        var res = _query.Query<BaseResStore>(addToken, parameters).FirstOrDefault();
                        result.Message = res.Message;
                        if(res.ResultStatus != -1)
                        {
                            _redisProvider.SetValueByKey($"token {res.ResultStatus}", jwt,1 );
                            result.Data = jwt;
                        }
                        else
                        {
                            result._code = -302;
                        }
                    }
                    else
                    {
                        result.Message = "Mật khẩu không chính xác";
                        result._code = -302;
                        return result;
                    }
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
