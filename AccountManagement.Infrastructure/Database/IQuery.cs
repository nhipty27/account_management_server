namespace AccountManagement.Infrastructure.Database
{
    public interface IQuery
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>( string sql, object param = null, int commandTimeout = 20);
        Task<IEnumerable<T>> QueryAsync<T>( string sql, object param = null, int commandTimeout = 20);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        int ExecuteCommandText( string sql, object param = null, int commandTimeout = 20);
    }
}
