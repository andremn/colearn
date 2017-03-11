using System.Text;

namespace FinalProject.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveDiacritics(this string input)
        {
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(input);

            return Encoding.ASCII.GetString(bytes);
        }
    }
}