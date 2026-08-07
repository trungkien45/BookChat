namespace BookChat.AIChat
{
    public interface IAIChatWebProvider
    {
        public string Name { get; }
        public string Description { get; }
        public string Icon { get; }
        public string Url { get; }
    }
}
