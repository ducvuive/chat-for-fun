using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class MessageViewModel
    {
        [Required]
        public string Content { get; set; }
        public string Timestamp { get; set; }
        public string From { get; set; }
        public string Username { get; set; }
        [Required]
        public string Room { get; set; }
        public string Avatar { get; set; }
        public bool HasMoreMessages { get; set; }
    }
}
