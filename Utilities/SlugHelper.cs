using System.Text;
using System.Text.RegularExpressions;

public static class SlugHelper
{
    public static string GenerateSlug(string phrase)
    {
        string str = phrase.ToLower();
        // Xóa dấu tiếng Việt
        str = Regex.Replace(str, @"[áàảạãăắằẳẵặâấầẩẫậ]", "a");
        str = Regex.Replace(str, @"[éèẻẽẹêếềểễệ]", "e");
        str = Regex.Replace(str, @"[iíìỉĩị]", "i");
        str = Regex.Replace(str, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
        str = Regex.Replace(str, @"[uúùủũụưứừửữự]", "u");
        str = Regex.Replace(str, @"[yýỳỷỹỵ]", "y");
        str = Regex.Replace(str, @"[đ]", "d");

        // Xóa ký tự đặc biệt
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        // Chuyển khoảng trắng thành gạch ngang
        str = Regex.Replace(str, @"\s+", "-").Trim();

        return str;
    }
}