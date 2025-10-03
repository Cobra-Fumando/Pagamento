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
                bool emaiValido = adress.Address == email;

                if(!emaiValido) return false;
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
