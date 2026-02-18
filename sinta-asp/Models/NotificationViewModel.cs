using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using sinta_asp.Data;


namespace sinta_asp.Models
{
    [Table("notification_view_model")]
    public class NotificationViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public bool IsRead { get; set; }
    }
}