using System.Text.RegularExpressions;

namespace Pic.Classes
{
    public class VerificarRegex
    {
        public static bool FormatoCpf(string cpf, out string CpfReplace)
        {
            bool Verificado = Regex.IsMatch(cpf, @"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
            CpfReplace = Regex.Replace(cpf, @"[.\-]", "");
            if (!Verificado) return false;
            return true;
        }

        public static bool FormatoTelefone(string telefone, out string TelefoneReplace)
        {
            bool Verificado = Regex.IsMatch(telefone, @"^\(\d{2}\) \d{4,5}-\d{4}$");
            TelefoneReplace = Regex.Replace(telefone, @"[()\s-]", "");
            if (!Verificado) return false;
            return true;
        }
    }
}
