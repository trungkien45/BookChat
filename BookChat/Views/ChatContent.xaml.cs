using BookChat.AIChat;
using System.ComponentModel;

namespace BookChat.Views;

public partial class ChatContent : ContentView
{
    private readonly List<IAIChatWebProvider> providers = typeof(IAIChatWebProvider).Assembly
    .GetTypes()
    .Where(t =>
        typeof(IAIChatWebProvider).IsAssignableFrom(t) &&
        t.IsClass &&
        !t.IsAbstract)
    .Select(t => (IAIChatWebProvider)Activator.CreateInstance(t)!)
    .ToList();
    private IAIChatWebProvider? selectedProvider;

    public ChatContent()
    {
        InitializeComponent();
        BindingContext = this;
        var provider = Preferences.Get(Const.appChatProvider, string.Empty);
        SelectedProvider = providers.FirstOrDefault(p => p.Name == provider);
    }
    public IAIChatWebProvider? SelectedProvider
    {
        get => selectedProvider;
        set
        {
            Preferences.Set(Const.appChatProvider, value?.Name ?? string.Empty);
            selectedProvider = value;
            if (selectedProvider != null)
            {
                xWebChat.Source = selectedProvider.Url;
            }
            OnPropertyChanged(nameof(SelectedProvider));
        }
    }
    public List<IAIChatWebProvider> Providers => providers;
}