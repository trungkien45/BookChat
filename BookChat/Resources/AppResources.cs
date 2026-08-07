using System.Globalization;

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
            { "NoLibPathConfigured", "No LibPath configured. Open settings to choose a folder." },
            { "FolderEmpty", "Folder is empty." },
            { "FolderReadError", "Folder is empty or could not be read." },
            { "PathNotFound", "Path not found or not accessible." },
            { "ErrorReadingPath", "Error reading path: {0}" },
            { "ErrorReadingContentUri", "Error reading content URI: {0}" },
            { "PickFolderFailed", "Could not determine full path on this platform. Please enter the folder path manually." },
            { "PleaseSelectLanguage", "Please select a language." },
            { "LanguageApplied", "Language applied. Restart the app to fully apply changes." },
            { "Cancel", "Cancel" },
            { "Ok", "OK" },
            { "Info", "Info" },
            { "Error", "Error" },
            { "Language", "Language" },
            { "Yes", "Yes" },
            { "No", "No" },
            { "Confirm", "Confirm" },
            { "MenuOpen", "Open" },
            { "MenuDelete", "Delete" },
            { "MenuRename", "Rename" },
            { "MenuOpenInNewView", "Open In New View" },
            { "MenuNewFolder", "New Folder" },
            { "CreateFolderNotSupported", "Not supported" },
            { "CreateFolderNotSupportedMessage", "Creating folders in this location is not supported." },
            { "OpenInNewViewNotSupported", "Opening a new view is not supported for this location." },
            { "NewFolderTitle", "New Folder" },
            { "NewFolderPrompt", "Folder name:" },
            { "NewFolderPlaceholder", "New folder" },
            { "MenuCloseView", "Close View" },
            { "RenameTitle", "Rename" },
            { "RenamePrompt", "New name:" },
            { "ConfirmDeleteMessage", "Delete '{0}'?" },
            { "ConfirmMoveMessage", "Move '{0}' to the other view?" },
            { "AlreadyInNewViewMessage", "This item is already in the other view." },
            { "FolderPickTitle", "Pick any file inside the folder you want to use" },
            { "FolderPickFailedMessage", "Folder pick failed: {0}" },
            { "LibPathEmptyMessage", "Lib Path is empty. Please enter or pick a folder." },
            { "ApplyLanguageError", "Could not apply language: {0}" },
            { "MenuMoveNewView", "Move To New View" },
            { "TitleError", "Error" },
            { "OpenPDFError", "Could not open PDF file"  },
            { "Library", "Library" },
            { "Bookmarks", "Bookmarks" },
            { "Notes", "Notes" },
            { "UnpinSidebar", "Unpin (Auto-hide)" },
            { "PinSidebar", "Pin sidebar" },
            { "ChatPanel", "Chat" },
            { "UnpinChat", "Unpin (Auto-hide)" },
            { "PinChat", "Pin chat" },
            { "LoadingPDF", "Loading PDF..." },
            { "BookmarkListTitle", "Bookmarks List" },
            { "NoteListTitle", "Notes List" },

            { "AddButton", "+ Add" },

            { "NoBookmarksYet", "No bookmarks yet" },
            { "NoNotesYet", "No notes yet" },

            { "NoBookmarksDetail", "Click '+ Add' above to bookmark the current page." },
            { "NoNotesDetail", "Click '+ Add' above to note the current page." },
            
            { "EditBookmark", "Edit bookmark" },
            { "EditNote", "Edit note" },

            { "DeleteBookmark", "Delete bookmark" },
            { "DeleteNote", "Delete note" },
            
            { "PageFormat", "Page {0}" },
            { "Edit", "Edit" }
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
            { "NoLibPathConfigured", "Chưa cấu hình LibPath. Mở cài đặt để chọn thư mục." },
            { "FolderEmpty", "Thư mục trống." },
            { "FolderReadError", "Thư mục trống hoặc không thể đọc." },
            { "PathNotFound", "Đường dẫn không tồn tại hoặc không thể truy cập." },
            { "ErrorReadingPath", "Lỗi đọc đường dẫn: {0}" },
            { "ErrorReadingContentUri", "Lỗi đọc URI nội dung: {0}" },
            { "PickFolderFailed", "Không thể lấy đường dẫn đầy đủ trên nền tảng này. Vui lòng nhập thủ công." },
            { "PleaseSelectLanguage", "Vui lòng chọn ngôn ngữ." },
            { "LanguageApplied", "Đã áp dụng ngôn ngữ. Khởi động lại ứng dụng để áp dụng đầy đủ." },
            { "Cancel", "Hủy" },
            { "Ok", "OK" },
            { "Info", "Thông tin" },
            { "Error", "Lỗi" },
            { "Language", "Ngôn ngữ" },
            { "Yes", "Có" },
            { "No", "Không" },
            { "Confirm", "Xác nhận" },
            { "MenuOpen", "Mở" },
            { "MenuDelete", "Xóa" },
            { "MenuRename", "Đổi tên" },
            { "MenuOpenInNewView", "Mở trong khung mới" },
            { "MenuNewFolder", "Tạo thư mục mới" },
            { "CreateFolderNotSupported", "Không hỗ trợ" },
            { "CreateFolderNotSupportedMessage", "Không hỗ trợ tạo thư mục tại vị trí này." },
            { "OpenInNewViewNotSupported", "Không thể mở cửa sổ mới cho vị trí này." },
            { "NewFolderTitle", "Tạo thư mục mới" },
            { "NewFolderPrompt", "Tên thư mục:" },
            { "NewFolderPlaceholder", "Thư mục mới" },
            { "MenuCloseView", "Đóng khung" },
            { "RenameTitle", "Đổi tên" },
            { "RenamePrompt", "Tên mới:" },
            { "ConfirmDeleteMessage", "Xóa '{0}'?" },
            { "ConfirmMoveMessage", "Di chuyển '{0}' sang khung bên kia?" },
            { "AlreadyInNewViewMessage", "Mục này đã ở khung bên kia." },
            { "FolderPickTitle", "Chọn bất kỳ tệp nào bên trong thư mục bạn muốn dùng" },
            { "FolderPickFailedMessage", "Không thể chọn thư mục: {0}" },
            { "LibPathEmptyMessage", "Đường dẫn Lib Path trống. Vui lòng nhập hoặc chọn thư mục." },
            { "ApplyLanguageError", "Không thể áp dụng ngôn ngữ: {0}" },
            { "MenuMoveNewView", "Di chuyển sang bên kia" },
            { "TitleError",  "Lỗi" },
            { "OpenPDFError", "Không thể mở tệp PDF" },
            { "Library", "Thư viện" },
            { "Bookmarks", "Dấu trang" },
            { "Notes", "Ghi chú" },
            { "UnpinSidebar", "Bỏ ghim (tự động ẩn)" },
            { "PinSidebar", "Ghim thanh bên" },
            { "ChatPanel", "Trò chuyện" },
            { "UnpinChat", "Bỏ ghim (tự động ẩn)" },
            { "PinChat", "Ghim khung chat" },
            { "LoadingPDF", "Đang tải PDF..." },
            { "BookmarkListTitle", "Danh sách dấu trang" },
            { "NoteListTitle", "Danh sách ghi chú" },

            { "AddButton", "+ Thêm" },
            { "NoBookmarksYet", "Chưa có dấu trang nào" },
            { "NoNotesYet", "Chưa có ghi chú nào" },
            
            { "NoBookmarksDetail", "Nhấn nút '+ Thêm' phía trên để tạo dấu trang cho trang hiện tại." },
            { "NoNotesDetail", "Nhấn nút '+ Thêm' phía trên để tạo ghi chú cho trang hiện tại." },

            { "EditBookmark", "Sửa dấu trang" },
            { "EditNote", "Sửa ghi chú" },
            
            { "DeleteBookmark", "Xóa dấu trang" },
            { "DeleteNote", "Xóa ghi chú" },

            { "PageFormat", "Trang {0}" },
            { "Edit", "Sửa" }
        };

        static Dictionary<string, string> CurrentDict
        {
            get
            {
                var name = CultureInfo.CurrentUICulture?.Name ?? Const.ENLang;
                if (name.StartsWith(Const.VNlangPrefix)) return vi;
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
        public static string NoLibPathConfigured => Get("NoLibPathConfigured");
        public static string FolderEmpty => Get("FolderEmpty");
        public static string FolderReadError => Get("FolderReadError");
        public static string PathNotFound => Get("PathNotFound");
        public static string ErrorReadingPath => Get("ErrorReadingPath");
        public static string ErrorReadingContentUri => Get("ErrorReadingContentUri");
        public static string Cancel => Get("Cancel");
        public static string Ok => Get("Ok");
        public static string Info => Get("Info");
        public static string Error => Get("Error");
        public static string Language => Get("Language");
        public static string Yes => Get("Yes");
        public static string No => Get("No");
        public static string Confirm => Get("Confirm");
        public static string MenuOpen => Get("MenuOpen");
        public static string MenuDelete => Get("MenuDelete");
        public static string MenuRename => Get("MenuRename");
        public static string MenuOpenInNewView => Get("MenuOpenInNewView");
        public static string MenuNewFolder => Get("MenuNewFolder");
        public static string MenuCloseView => Get("MenuCloseView");
        public static string CreateFolderNotSupported => Get("CreateFolderNotSupported");
        public static string CreateFolderNotSupportedMessage => Get("CreateFolderNotSupportedMessage");
        public static string OpenInNewViewNotSupported => Get("OpenInNewViewNotSupported");
        public static string NewFolderTitle => Get("NewFolderTitle");
        public static string NewFolderPrompt => Get("NewFolderPrompt");
        public static string NewFolderPlaceholder => Get("NewFolderPlaceholder");
        public static string RenameTitle => Get("RenameTitle");
        public static string RenamePrompt => Get("RenamePrompt");
        public static string ConfirmDeleteMessage => Get("ConfirmDeleteMessage");
        public static string ConfirmMoveMessage => Get("ConfirmMoveMessage");
        public static string AlreadyInNewViewMessage => Get("AlreadyInNewViewMessage");
        public static string FolderPickTitle => Get("FolderPickTitle");
        public static string FolderPickFailedMessage => Get("FolderPickFailedMessage");
        public static string LibPathEmptyMessage => Get("LibPathEmptyMessage");
        public static string ApplyLanguageError => Get("ApplyLanguageError");
        public static string MenuMoveNewView => Get("MenuMoveNewView");
        public static string TitleError => Get("TitleError");
        public static string OpenPDFError => Get("OpenPDFError");

        public static string Library => Get("Library");

        public static string Bookmarks => Get("Bookmarks");

        public static string Notes => Get("Notes");

        public static string UnpinSidebar => Get("UnpinSidebar");

        public static string PinSidebar => Get("PinSidebar");

        public static string ChatPanel => Get("ChatPanel");

        public static string UnpinChat => Get("UnpinChat");

        public static string PinChat => Get("PinChat");

        public static string LoadingPDF => Get("LoadingPDF");

        public static string BookmarkListTitle => Get("BookmarkListTitle");
        public static string NoteListTitle => Get("NoteListTitle");

        public static string AddButton => Get("AddButton");

        public static string NoBookmarksYet => Get("NoBookmarksYet");
        public static string NoNotesYet => Get("NoNotesYet");

        public static string NoBookmarksDetail => Get("NoBookmarksDetail");
        public static string NoNotesDetail => Get("NoNotesDetail");

        public static string EditBookmark => Get("EditBookmark");
        public static string EditNote => Get("EditNote");

        public static string DeleteBookmark => Get("DeleteBookmark");
        public static string DeleteNote => Get("DeleteNote");

        public static string PageFormat => Get("PageFormat");

        public static string Edit => Get("Edit");

        static string Get(string key)
        {
            var dict = CurrentDict;
            if (dict.TryGetValue(key, out var v)) return v;
            return key;
        }
    }
}
