using System.Globalization;
using System.Collections.Generic;

namespace BookChat.Resources
{
    // Simple resource provider that returns text based on CurrentUICulture.
    // This is a lightweight alternative to .resx for this example and supports en-US and vi-VN.
    public static class AppResources
    {
        static readonly Dictionary<string, string> en = new()
        {
            { "SettingsTitle", "Settings" },
            { "LibPathLabel", "Lib Path" },
            { "LibPathPlaceholder", "Select or enter folder path" },
            { "BrowseButton", "Browse" },
            { "SaveLibPathButton", "Save Lib Path" },
            { "AppLanguageLabel", "App Language" },
            { "SelectLanguageTitle", "Select language" },
            { "ApplyLanguageButton", "Apply Language" },
            { "ConfigureInfo", "Configure your app settings here." },
            { "SavedLibPathMessage", "Lib Path saved." },
            { "PickFolderFailed", "Could not determine full path on this platform. Please enter the folder path manually." },
            { "PleaseSelectLanguage", "Please select a language." },
            { "LanguageApplied", "Language applied. Restart the app to fully apply changes." }
        };

        static readonly Dictionary<string, string> vi = new()
        {
            { "SettingsTitle", "Cài đặt" },
            { "LibPathLabel", "Thư mục Lib" },
            { "LibPathPlaceholder", "Chọn hoặc nhập đường dẫn thư mục" },
            { "BrowseButton", "Duyệt" },
            { "SaveLibPathButton", "Lưu Lib Path" },
            { "AppLanguageLabel", "Ngôn ngữ ứng dụng" },
            { "SelectLanguageTitle", "Chọn ngôn ngữ" },
            { "ApplyLanguageButton", "Áp dụng ngôn ngữ" },
            { "ConfigureInfo", "Cấu hình cài đặt ứng dụng tại đây." },
            { "SavedLibPathMessage", "Đã lưu Lib Path." },
            { "PickFolderFailed", "Không thể lấy đường dẫn đầy đủ trên nền tảng này. Vui lòng nhập thủ công." },
            { "PleaseSelectLanguage", "Vui lòng chọn ngôn ngữ." },
            { "LanguageApplied", "Đã áp dụng ngôn ngữ. Khởi động lại ứng dụng để áp dụng đầy đủ." }
        };

        static Dictionary<string, string> CurrentDict
        {
            get
            {
                var name = CultureInfo.CurrentUICulture?.Name ?? "en-US";
                if (name.StartsWith("vi")) return vi;
                return en;
            }
        }

        public static string SettingsTitle => Get("SettingsTitle");
        public static string LibPathLabel => Get("LibPathLabel");
        public static string LibPathPlaceholder => Get("LibPathPlaceholder");
        public static string BrowseButton => Get("BrowseButton");
        public static string SaveLibPathButton => Get("SaveLibPathButton");
        public static string AppLanguageLabel => Get("AppLanguageLabel");
        public static string SelectLanguageTitle => Get("SelectLanguageTitle");
        public static string ApplyLanguageButton => Get("ApplyLanguageButton");
        public static string ConfigureInfo => Get("ConfigureInfo");
        public static string SavedLibPathMessage => Get("SavedLibPathMessage");
        public static string PickFolderFailed => Get("PickFolderFailed");
        public static string PleaseSelectLanguage => Get("PleaseSelectLanguage");
        public static string LanguageApplied => Get("LanguageApplied");

        static string Get(string key)
        {
            var dict = CurrentDict;
            if (dict.TryGetValue(key, out var v)) return v;
            return key;
        }
    }
}
