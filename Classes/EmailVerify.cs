using System.Net.Mail;

namespace Pic.Classes
{
    public class EmailVerify
    {
        public static bool IsValidEmail(string email)
        {
            try
            {
                var adress = new MailAddress(email);
                return adress.Address == email && email.EndsWith(".com");
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
