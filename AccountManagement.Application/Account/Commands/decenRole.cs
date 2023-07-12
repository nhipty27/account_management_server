using AccountManagement.Application.Account.Dtos;
using AccountManagement.Application.Common.Models;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using Dapper;
using FluentValidation;
using System.Data;

namespace AccountManagement.Application.Account.Commands
{
    public class decenRole
    {
        ///<summary>
        ///script delete account
        ///</summary>
        const string decenRoleScript = @"
            DECLARE
                @ResultStatus INT = -1,
                @Message  NVARCHAR(300) = N'';
            IF NOT EXISTS (
                SELECT * FROM USERS WHERE id = @id
            )
                BEGIN
                     SELECT @ResultStatus = -1, @Message = N'Tài khoản không tồn tại!';
                     GOTO lblResult;
                END;
            UPDATE USER_ROLE 
            SET roleId = (SELECT id FROM ROLES WHERE name = @role), createBy = @createBy, createAt = GETDATE() 
            WHERE userId = @id
            IF @@ROWCOUNT <= 0
                BEGIN
                    SELECT @ResultStatus = -1, @Message = N'Cập nhật thất bại';
                    GOTO lblResult;
                END;
            SELECT @ResultStatus = 1, @Message = N'Cập nhật thành công';
            lblResult:
                SELECT @ResultStatus AS ResultStatus, @Message AS Message
        ";


        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<string>
        {
            public decenRoleRequest Data { get; set; }
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

            public Handler(IQuery query)
            {
                _query = query;
            }
            public async Task<ResultObject<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = new ResultObject<string>();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@id", request.Data.id, DbType.Int32);
                parameters.Add("@createBy", request.Data.createBy, DbType.Int32);
                parameters.Add("@role", request.Data.role, DbType.String);

                try
                {
                    var res = _query.Query<BaseResStore>(decenRoleScript, parameters).FirstOrDefault();
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
