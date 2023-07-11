using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace AccountManagement.Infrastructure.Database
{
    public class SqlServer : IQuery
    {
        private readonly string _connectionString;

        public SqlServer()
        {
            //_connectionString = "Server=LAPTOP-H02FPD4H\\NHIYENPHAN99;Database=AccountManagement;TrustServerCertificate=True;Trusted_Connection=True";
            _connectionString = "Server=.;Database=AccountManagement;TrustServerCertificate=True;Trusted_Connection=True";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, object param = null, int commandTimeout = 20)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var result = conn.Query<T>(sql, param, commandTimeout: commandTimeout);
                    return result.ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataprovider"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, int commandTimeout = 20)
        {
            try
            {
                using (SqlConnection conn = new  SqlConnection(_connectionString))
                {
                    conn.Open();
                    var result = await conn.QueryAsync<T>(sql, param, commandTimeout: commandTimeout);
                    return result.ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int ExecuteCommandText( string sql, object param = null, int commandTimeout = 20)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    return conn.Execute(sql, param, commandType: CommandType.Text, commandTimeout: commandTimeout);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
