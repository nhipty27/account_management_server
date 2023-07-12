using AccountManagement.Application.Account.Dtos;
using AccountManagement.Application.Common.Models;
using AccountManagement.Infrastructure.Core.Models;
using AccountManagement.Infrastructure.Database;
using Dapper;
using FluentValidation;
using System.Data;

namespace AccountManagement.Application.Account.Commands
{
    public class GetAccount
    {
        /// <summary>
        /// Script lấy danh sách tài khoản theo role
        /// </summary>
        const string getAccounts = @"
            DECLARE
                @startRow INT = (@page -1)*@pageSize;

            CREATE TABLE #ROLE(
             NAME [varchar](50)
            )            
            INSERT INTO #ROLE VALUES('USER')
            --xét role.
            IF @role <> 'USER'  
                BEGIN
                    INSERT INTO #ROLE(NAME) VALUES('MANAGER')
                    IF @role <> 'MANAGER'
                        INSERT INTO #ROLE(NAME) VALUES('ADMIN')
                END
            --get accounts
            SELECT u.id as id, u.email as email, u.name as name, u.address as address, u.dayOfBirth as dayOfBirth, u.gender as gender, u.createAt as createAt, r.name as role
            INTO #TMP
            FROM users u
            LEFT JOIN user_role ur ON u.id = ur.userId
            LEFT JOIN roles r ON r.id = ur.roleId
            WHERE r.name IN (
                SELECT * FROM #ROLE
            ) AND u.id <> @id; 
            --sort
            DECLARE @sqlQuery NVARCHAR(MAX);
            SET @sqlQuery = N'SELECT *  FROM ( 
                            SELECT *, ROW_NUMBER() OVER (ORDER BY '+ @sortType + ' ' + @sortValue+ ' ) AS RowNum
                            FROM #TMP       
                            --search
                            WHERE '+ @searchType + ' LIKE N''%'  + @searchValue + N'%''
                            ) AS SUBQUERY
                            WHERE RowNum  > ' + CAST((@page -1)*@pageSize AS NVARCHAR(10)) + '
                            AND RowNum  <= ' + CAST(@page*@pageSize AS NVARCHAR(10)) + 
                            ';';

            EXEC sp_executesql @sqlQuery;
                DROP TABLE #TMP                
                DROP TABLE #ROLE
        ";

        const string getRole = @"SELECT R.NAME 
                        FROM ROLES R, USER_ROLE UR
                        WHERE R.id = UR.roleId AND UR.userId = @id
                    ";


        const string getPageNumber = @"
                        CREATE TABLE #ROLE(
             NAME [varchar](50)
            )            
            INSERT INTO #ROLE VALUES('USER')
            --xét role.
            IF @role <> 'USER'  
                BEGIN
                    INSERT INTO #ROLE(NAME) VALUES('MANAGER')
                    IF @role <> 'MANAGER'
                        INSERT INTO #ROLE(NAME) VALUES('ADMIN')
                END
            --get accounts
            SELECT u.id as id, u.email as email, u.name as name, u.address as address, u.dayOfBirth as dayOfBirth, u.gender as gender, u.createAt as createAt, r.name as role
            INTO #TMP
            FROM users u
            LEFT JOIN user_role ur ON u.id = ur.userId
            LEFT JOIN roles r ON r.id = ur.roleId
            WHERE r.name IN (
                SELECT * FROM #ROLE
            ) AND u.id <> @id; 
            --sort
            DECLARE @sqlQuery NVARCHAR(MAX);
            SET @sqlQuery = N' SELECT *
                            FROM #TMP       
                            --search
                            WHERE '+ @searchType + ' LIKE N''%'  + @searchValue + N'%'' ;';

            EXEC sp_executesql @sqlQuery;
                DROP TABLE #TMP                
                DROP TABLE #ROLE
        ";
        /// <summary>
        /// Request input
        /// </summary>
        public class Command : IRequestWrapper<AccountsRespone>
        {
                public getAccountsRequest Data { get; set; }
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

        public class Handler : IRequestHandlerWrapper<Command, AccountsRespone>
        {
            private readonly IQuery _query;

            public Handler(IQuery query)
            {
                _query = query;
            }

            public async Task<ResultObject<AccountsRespone>> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = new ResultObject<AccountsRespone>();
                try
                {
                    string role = _query.Query<string>(getRole, new {id = request.Data.id}).FirstOrDefault();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@role", role, DbType.String);
                    parameters.Add("@id", request.Data.id, DbType.Int32);
                    parameters.Add("@searchType", request.Data.searchType ?? "", DbType.String);
                    parameters.Add("@searchValue", request.Data.searchValue ?? "", DbType.String);
                    parameters.Add("@sortType", request.Data.sortType ?? "createAt", DbType.String);
                    parameters.Add("@sortValue", request.Data.sortValue ?? "DESC", DbType.String);
                    parameters.Add("@page", request.Data.page , DbType.Int32);
                    parameters.Add("@pageSize", request.Data.pageSize , DbType.Int32);
                    if(role == null)
                    {
                        result.Message = "Thất bại";
                        result._code = -130;
                        return result;
                    }
                    List<UserDto> res  = _query.Query<UserDto>(getAccounts, parameters).ToList();
                    List <UserDto> numAcc = _query.Query<UserDto>(getPageNumber,  new { 
                                                            role, 
                                                            searchType = request.Data.searchType, 
                                                            searchValue = request.Data.searchValue, 
                                                            id = request.Data.id}
                                                    ).ToList();
                    result.Data = new AccountsRespone();
                    result.Data.users = res;
                    result.Data.size = numAcc.Count();
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
