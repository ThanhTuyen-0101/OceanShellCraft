using System.Text;
using System.Text.RegularExpressions;

namespace OceanShellCraft.Helpers
{
    public static class StringHelpers
    {
        // Chữ 'this' ở đây giúp biến hàm này thành một Extension Method
        public static string ToSlug(this string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            // 1. Chuyển đổi Unicode để tách dấu ra khỏi chữ cái
            string temp = value.Normalize(NormalizationForm.FormD);

            // 2. Xóa các dấu tiếng Việt
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            value = regex.Replace(temp, string.Empty);

            // 3. Xử lý riêng chữ đ/Đ của tiếng Việt
            value = value.Replace('\u0111', 'd').Replace('\u0110', 'D');

            // 4. Chuyển tất cả thành chữ thường
            value = value.ToLowerInvariant();

            // 5. Xóa các ký tự đặc biệt (chỉ giữ lại chữ cái, số, khoảng trắng và gạch ngang)
            value = Regex.Replace(value, @"[^a-z0-9\s-]", "");

            // 6. Chuyển khoảng trắng thành dấu gạch ngang
            value = Regex.Replace(value, @"\s+", "-").Trim();

            // 7. Gộp các dấu gạch ngang liền kề (ví dụ: a---b thành a-b)
            value = Regex.Replace(value, @"-+", "-");

            return value;
        }
    }
}