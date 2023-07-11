namespace AccountManagement.Infrastructure.Core.Models
{
    public class ResultObject<T>
    {
        public int _code {  get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } 
        public ResultObject(int code, string message, T data)
        {
            _code = code;
            Message = message;
            Data = data;
        }

        public ResultObject()
        {
            _code = 200;
            Message = "Thành công";
        }

        public static ResultObject<T> Ok<T>(T data, string message = "Thành công.")
        {
            var msg = message;
            return new ResultObject<T>(200, msg, data);
        }

        /// <summary>
        /// OkAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Task<ResultObject<T>> OkAsync<T>(T data, string message = "Thành công.")
        {
            return Task.FromResult(Ok(data, message));
        }

    }


}
