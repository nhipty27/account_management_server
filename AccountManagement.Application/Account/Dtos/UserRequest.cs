namespace AccountManagement.Application.Account.Dtos
{
    public class UserRequest
    {
        public string email { get; set; }
        public string name { get; set; }
        public string? password { get; set; }
        public int? createBy { get; set; } = -1;
        public int? id { get; set; }
        public string? role { get; set; }
        public bool? gender { get; set; }
        public string? address { get; set; }
        public DateTime? dayOfBirth { get; set; }
    }

    public class decenRoleRequest
    {
        public int id { get; set; }
        public string role { get; set; }
        public int createBy { get; set; }
    }

    public class deleteRequest
    {
        public int id { get; set; }
    }

}
