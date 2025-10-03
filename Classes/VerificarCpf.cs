using System.Text.RegularExpressions;

namespace Pic.Classes
{
    public class VerificarCpf
    {
        public static bool FormatoCpf(string cpf, out string CpfReplace)
        {
            try
            {
                bool Verificado = Regex.IsMatch(cpf, @"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
                CpfReplace = Regex.Replace(cpf, @"[.\-]", "");
                if (!Verificado) return false;
                return true;
            }
            catch (Exception ex)
            { 
                CpfReplace = string.Empty;
                return false;
            }
        }
    }
}
