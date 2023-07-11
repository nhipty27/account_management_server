using Microsoft.AspNetCore.Http;

namespace AccountManagement.Infrastructure.Core.Cookie
{
    public static class CookieHelper
    {
       
        public static void setCookie( HttpContext context ,string key, string value, int expireDays)
        {
            var option = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(expireDays),
            };

            if(context.Request.Cookies.ContainsKey(key))
            {
                context.Response.Cookies.Delete(key);
            }
            //context.Response.Cookies.Append(key, value);
            context.Response.Cookies.Append(key, value);
        }

        public static string getCookie ( HttpContext context ,string key)
        {
            if(context.Request.Cookies.TryGetValue(key, out var value)) 
            {
                return value;
            }
            return null;
        }

        public static void deleteCookie ( HttpContext context ,string key)
        {
            if (context.Request.Cookies.ContainsKey(key))
            {
                context.Response.Cookies.Delete(key);
            }
        }
    }
}
