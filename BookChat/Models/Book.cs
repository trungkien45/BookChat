namespace BookChat.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Path { get; set; } = null!;

        public int ReadingPage { get; set; }
    }
}
