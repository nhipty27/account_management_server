
namespace AccountManagement.Application.Common.Models
{
    /// <summary>
    /// Response cơ bản của Stored trả về
    /// </summary>
    public class BaseResStore
    {
        public int ResultStatus { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Response cơ bản của Stored trả về vầ danh sách
    /// </summary>
    /// 
    public class DataResStore : BaseResStore
    {
        public IEnumerable<UserDto>? Users { get; set; } 
    }
}
