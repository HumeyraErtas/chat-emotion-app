namespace ChatBackend.Models
{
    public class Message
    {
        public int Id { get; set; }
        public required string Text { get; set; } = string.Empty;
        public string Emotion { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Kullanıcı ile ilişki
        public int UserId { get; set; }
        public User? User { get; set; } // Navigation property
    }
}
