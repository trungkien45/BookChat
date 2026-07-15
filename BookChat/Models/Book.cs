namespace BookChat.Models
{
    public class Book : Entity
    {
        public string Path { get; set; } = null!;

        public int ReadingPage { get; set; }
    }
}
