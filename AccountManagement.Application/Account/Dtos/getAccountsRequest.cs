using AccountManagement.Application.Common.Models;

namespace AccountManagement.Application.Account.Dtos
{
    public class getAccountsRequest
    {
        public int id { get; set; }
        public string? searchType { get; set; }
        public string? searchValue { get; set; }
        public string? sortType { get; set; }
        public string? sortValue { get; set; }
        public int? page { get; set; } = 1;
        public int? pageSize { get; set; } = 5;
    }

    public class AccountsRespone
    {
        public int size { get; set; }
        public List<UserDto> users { get; set; }

        public AccountsRespone()
        {
            this.users = new List<UserDto>();
            this.size = 0;
        }

        public AccountsRespone(int size, List<UserDto> users)
        {
            this.size = size;
            this.users = users;
        }
    }
}
