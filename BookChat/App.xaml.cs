using System.Globalization;

namespace BookChat
{
    public partial class App : Application
    {
        public App()
        {
            ApplySavedLanguage();
            InitializeComponent();
        }

        private static void ApplySavedLanguage()
        {
            try
            {
                var language = Preferences.Get("AppLanguage", CultureInfo.CurrentUICulture?.Name ?? "en-US");
                if (string.IsNullOrWhiteSpace(language))
                    return;

                var culture = new CultureInfo(language);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch
            {
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}