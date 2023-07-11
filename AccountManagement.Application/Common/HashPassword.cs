
namespace AccountManagement.Application.Common
{
    public static class HashPassword 
    {
        public static DateTime calDate(int n)
        {
            DateTime curr = DateTime.Now;
            DateTime futureDate = curr.AddDays(n);
            return futureDate;
        }

        public static string CreatePasswordHash(string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return hashedPassword;
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
