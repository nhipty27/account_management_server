using AccountManagement.Application.Account.Dtos;
using AccountManagement.Application.Auth.Dtos;
using AccountManagement.Application.Common;
using AccountManagement.Application.Common.Models;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using Dapper;
using FluentValidation;
using System;
using System.Data;

namespace AccountManagement.Application.Auth.Commands
{
    public class ChangePassword
    {
        const string changPw = @"
            DECLARE
                @ResultStatus INT = -1,
                @Message  NVARCHAR(300) = N'';
            --đăng xuất tất cả tài khoản
            IF (@isLogout = 1)
            BEGIN
                DELETE FROM USER_TOKEN WHERE userId = @id

                IF @@ROWCOUNT <= 0
                    BEGIN
                        SELECT @ResultStatus = -1, @Message = N'Đổi mật khẩu thất bại';
                        GOTO lblResult;
                    END;
            END

            UPDATE USERS SET password = @new_password
            
            SELECT @ResultStatus = 1, @Message = N'Đổi mật khẩu thành công';

            lblResult:
                SELECT @ResultStatus AS ResultStatus, @Message AS Message
        ";

        const string getOldPassword = @"
            SELECT password FROM USERS WHERE id = @id
        ";
        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<int>
        {
            public changePasswordRequest Data { get; set; }
        }

        /// <summary>
        /// Validate
        /// </summary>
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Data.id).GreaterThan(0).WithMessage("Id người dùng không hợp lệ");
                RuleFor(x => x.Data.newPassword.Length).GreaterThan(40).WithMessage("Chiều dài mật khẩu không quá 40 ký tự");
                RuleFor(x => x.Data.newPassword.Length).LessThan(6).WithMessage("Chiều dài mật khẩu tối thiểu 6 ký tự");
            }
        }

        public class Handler : IRequestHandlerWrapper<Command, int>
        {
            private readonly IQuery _query;

            public Handler(IQuery query)
            {
                _query = query;
            }
            public async Task<ResultObject<int>> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = new ResultObject<int>();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@id", request.Data.id, DbType.Int32);
                parameters.Add("@new_password", HashPassword.CreatePasswordHash(request.Data.newPassword) ?? "", DbType.String);
                parameters.Add("@isLogout", request.Data.isLogout, DbType.Boolean);
                try
                {
                    var oldPw = _query.Query<string>(getOldPassword, new {id = request.Data.id}).FirstOrDefault();
                    if (oldPw != null)
                    {
                        result.Message = "Tài khoản không tồn tại";
                        result._code = -130;
                    }
                    if(HashPassword.VerifyPassword(request.Data.oldPassword, oldPw))
                    {
                        var res = _query.Query<BaseResStore>(changPw, parameters).FirstOrDefault();
                        if (res.ResultStatus == -1)
                        {
                            result._code = -302;
                        }
                        result.Message = res.Message;
                        if(request.Data.isLogout == true)
                        {
                            result.Data = 0;
                        }
                        result.Data = 1;
                        return result;
                    }
                    else
                    {
                        result.Message = "Mật khẩu hiện tại không chính xác!";
                        result._code = -130;
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
