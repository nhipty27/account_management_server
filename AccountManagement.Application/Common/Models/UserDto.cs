namespace AccountManagement.Application.Common.Models
{
    public class UserDto
    {
        public int id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public bool gender { get; set; }
        public string address { get; set; }
        public string role { get; set; }
        public DateTime dayOfBirth { get; set; }
        public DateTime createAt { get; set; }
    }

    //public class UserRespone
    //{
    //    public UserDto user { get; set; }
    //    public string role { get; set; }
    //}
}
