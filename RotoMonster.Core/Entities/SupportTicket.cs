using System;
using System.ComponentModel.DataAnnotations;

namespace RotoMonster.Core
{
    public class SupportTicket
    {
        public int Id { get; set; }

        [Required, StringLength(64)]
        public string Token { get; set; }

        [Required, StringLength(40)]
        public string ZohoTicketId { get; set; }

        [StringLength(20)]
        public string TicketNumber { get; set; }

        [StringLength(450)]
        public string UserId { get; set; }

        [Required, StringLength(256)]
        public string Email { get; set; }

        [StringLength(200)]
        public string Subject { get; set; }

        [StringLength(10)]
        public string Sport { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
