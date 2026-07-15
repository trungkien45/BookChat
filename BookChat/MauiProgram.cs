using BookChat.Data.Providers;
using BookChat.Data.Providers.Sqlite;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace BookChat
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register Services
#if ANDROID
            builder.Services.AddSingleton<BookChat.StorageService.Inteface.IStogareService, BookChat.StorageService.Implement.AndroidStogareService>();
#elif WINDOWS
            builder.Services.AddSingleton<BookChat.StorageService.Inteface.IStogareService, BookChat.StorageService.Implement.WindowsStogareService>();
#endif
            
            builder.Services.AddSingleton<BookChat.Data.Providers.SqliteDatabase>();
            builder.Services.AddSingleton<IDbSessionFactory, SqliteDbSessionFactory>();
            builder.Services.AddSingleton<BookChat.Data.Service.IBookService, BookChat.Data.Service.BookService>();
            builder.Services.AddTransient<MainPage>();

            // Apply saved app language (if any) so resources use correct culture on startup
            try
            {
                var savedLang = Preferences.Get("AppLanguage", string.Empty);
                if (!string.IsNullOrWhiteSpace(savedLang))
                {
                    var ci = new CultureInfo(savedLang);
                    CultureInfo.DefaultThreadCurrentCulture = ci;
                    CultureInfo.DefaultThreadCurrentUICulture = ci;
                }
            }
            catch { }

            return builder.Build();
        }
    }
}
