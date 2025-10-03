using System.Text.RegularExpressions;

namespace Pic.Classes
{
    public class VerificarCpf
    {
        public static bool FormatoCpf(string cpf)
        {
            try
            {
                bool Verificado = Regex.IsMatch(cpf, @"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
                if (!Verificado) return false;
                return true;
            }
            catch (Exception ex)
            { 
                return false;
            }
        }
    }
}
