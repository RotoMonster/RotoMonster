using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using RotoMonsterExternalAPIs.Client.Models.Requests;
using RotoMonsterExternalAPIs.Client.Models.Results;
using RotoMonsterExternalAPIs.Client.Services.Zoho;

namespace RotoMonster.Data
{
    public class SupportCreateResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public SupportTicket Ticket { get; set; }
    }

    public class SupportTicketView
    {
        public SupportTicket Ticket { get; set; }
        public SupportTicketResult Status { get; set; }
        public SupportConversationResult Conversation { get; set; }

        public bool IsClosed => Status != null && Status.IsClosed;

        public bool WaitingOnCustomer =>
            !IsClosed && Conversation != null && Conversation.Messages.Count > 0 && !Conversation.Messages[^1].FromCustomer;
    }

    public class SupportService
    {
        private readonly RMSharedDbContext _db;
        private readonly ZohoDeskClient _zoho;

        public SupportService(RMSharedDbContext db, ZohoDeskClient zoho)
        {
            _db = db;
            _zoho = zoho;
        }

        public bool IsConfigured => _zoho.IsConfigured;

        public async Task<SupportCreateResult> CreateAsync(CreateSupportTicketRequest request, string userId, string sport)
        {
            var created = await _zoho.CreateTicketAsync(request);
            if (!created.Success)
                return new SupportCreateResult { Success = false, ErrorMessage = created.ErrorMessage };

            var ticket = new SupportTicket
            {
                Token = NewToken(),
                ZohoTicketId = created.TicketId,
                TicketNumber = created.TicketNumber,
                UserId = string.IsNullOrEmpty(userId) ? null : userId,
                Email = request.Email.Trim(),
                Subject = Truncate(request.Subject, 200),
                Sport = Truncate(sport, 10),
                CreatedDate = DateTime.UtcNow
            };

            _db.Set<SupportTicket>().Add(ticket);
            await _db.SaveChangesAsync();

            return new SupportCreateResult { Success = true, Ticket = ticket };
        }

        public async Task<SupportTicketView> GetAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var ticket = await _db.Set<SupportTicket>().AsNoTracking().FirstOrDefaultAsync(t => t.Token == token);
            if (ticket == null) return null;

            return new SupportTicketView
            {
                Ticket = ticket,
                Status = await _zoho.GetTicketAsync(ticket.ZohoTicketId),
                Conversation = await _zoho.GetConversationAsync(ticket.ZohoTicketId)
            };
        }

        public async Task<List<SupportTicket>> GetForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return new List<SupportTicket>();

            return await _db.Set<SupportTicket>().AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedDate)
                .Take(25)
                .ToListAsync();
        }

        public async Task<SupportTicketResult> ReplyAsync(string token, string message)
        {
            var ticket = await _db.Set<SupportTicket>().AsNoTracking().FirstOrDefaultAsync(t => t.Token == token);
            if (ticket == null)
                return new SupportTicketResult { Success = false, ErrorMessage = "Ticket not found." };

            return await _zoho.AddCustomerReplyAsync(ticket.ZohoTicketId, message);
        }

        private static string NewToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return value;
            value = value.Trim();
            return value.Length <= max ? value : value.Substring(0, max);
        }
    }
}
