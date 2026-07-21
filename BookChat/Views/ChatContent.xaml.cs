using BookChat.AIChat;

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
    public ChatContent()
	{
		InitializeComponent();
        BindingContext = this;
    }
    public IAIChatWebProvider? SelectedProvider {  get; set; }
    public List<IAIChatWebProvider> Providers => providers;
}