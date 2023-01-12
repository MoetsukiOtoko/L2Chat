using System.Text.RegularExpressions;

namespace L2JChat
{
    internal class Validate
    {
        public static bool IsValidLoginString(string username)
        {
            if (username.Length <= 16 && username.Length >= 4)
            {
                return Regex.IsMatch(username, @"^[a-zA-Z0-9]+$");
            }
            else
            {
                return false;
            }          
        }


        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}
