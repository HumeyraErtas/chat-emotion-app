namespace ChatBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = string.Empty;

        public List<Message> Messages { get; set; } = new List<Message>();
    }
}

