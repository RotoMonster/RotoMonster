using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using RotoMonsterExternalAPIs.Client.Models.Requests;

namespace RotoMonster.Pages.Support
{
    public class IndexModel : RMPageModel
    {
        public static readonly string[] Categories =
        {
            "Account or login",
            "Membership or billing",
            "League import",
            "Rankings or data",
            "Something is broken",
            "Other"
        };

        private readonly SupportService _support;
        private readonly UserManager<ApplicationUser> _users;
        private readonly IEmailSender _email;
        private readonly IConfiguration _config;
        private readonly ILogger<PageModel> _log;

        public IndexModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor, ILogger<PageModel> logger, SupportService support, IEmailSender email)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
            _support = support;
            _users = userManager;
            _email = email;
            _config = config;
            _log = logger;
        }

        [BindProperty]
        public SupportForm Form { get; set; } = new SupportForm();

        public List<SupportTicket> MyTickets { get; set; } = new List<SupportTicket>();

        public string ErrorMessage { get; set; }

        private static readonly Dictionary<string, string> Sites = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "bbm", "Basketball Monster" },
            { "basketball", "Basketball Monster" },
            { "bsm", "Baseball Monster" },
            { "baseball", "Baseball Monster" }
        };

        [BindProperty(Name = "site", SupportsGet = true)]
        public string SiteKey { get; set; }

        public string SiteLabel => !string.IsNullOrEmpty(SiteKey) && Sites.TryGetValue(SiteKey, out var name) ? name : null;

        public bool IsConfigured => _support.IsConfigured;

        public async Task OnGetAsync()
        {
            InitGet("Support");
            await LoadUserAsync(true);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            InitGet("Support");
            await LoadUserAsync(false);

            if (!string.IsNullOrEmpty(Form.Website))
                return RedirectToPage("/Support/Index");

            if (!Categories.Contains(Form.Category))
                ModelState.AddModelError("Form.Category", "Pick a category.");

            if (!ModelState.IsValid)
                return Page();

            if (!IsConfigured)
            {
                ErrorMessage = "Support isn't available right now. Please email support@rotomonster.com.";
                return Page();
            }

            var sport = (_config["Sport"] ?? "").ToUpperInvariant();

            var result = await _support.CreateAsync(new CreateSupportTicketRequest
            {
                Name = Form.Name,
                Email = Form.Email,
                Subject = Form.Subject,
                Description = Form.Description,
                Category = Form.Category,
                Site = SiteLabel ?? ("RotoMonster " + sport)
            }, CurrentUserId(), sport);

            if (!result.Success)
            {
                _log.LogWarning("Support ticket create failed: {Error}", result.ErrorMessage);
                ErrorMessage = "We couldn't submit your request. Please try again, or email support@rotomonster.com.";
                return Page();
            }

            var link = Url.Page("/Support/Status", null, new { token = result.Ticket.Token }, Request.Scheme);

            var emailed = false;
            try
            {
                await _email.SendEmailAsync(result.Ticket.Email,
                    "RotoMonster support request #" + result.Ticket.TicketNumber,
                    "<p>We got your request: <b>" + WebUtility.HtmlEncode(result.Ticket.Subject) + "</b></p>" +
                    "<p>You can check its status and reply here:<br><a href=\"" + link + "\">" + link + "</a></p>");
                emailed = true;
            }
            catch (System.Exception ex)
            {
                _log.LogWarning(ex, "Support link email failed for ticket {Ticket}", result.Ticket.TicketNumber);
            }

            return RedirectToPage("/Support/Status", new { token = result.Ticket.Token, created = true, emailed });
        }

        private string CurrentUserId()
        {
            return User?.Identity?.IsAuthenticated == true ? _users.GetUserId(User) : null;
        }

        private async Task LoadUserAsync(bool prefill)
        {
            var userId = CurrentUserId();
            if (userId == null) return;

            MyTickets = await _support.GetForUserAsync(userId);

            if (!prefill) return;

            var user = await _users.GetUserAsync(User);
            if (user == null) return;

            if (string.IsNullOrEmpty(Form.Email)) Form.Email = user.Email;
            if (string.IsNullOrEmpty(Form.Name)) Form.Name = user.UserName;
        }

        public class SupportForm
        {
            [Required, StringLength(80)]
            public string Name { get; set; }

            [Required, EmailAddress, StringLength(256)]
            public string Email { get; set; }

            [Required]
            public string Category { get; set; }

            [Required, StringLength(150)]
            public string Subject { get; set; }

            [Required, StringLength(5000, MinimumLength = 10)]
            public string Description { get; set; }

            public string Website { get; set; }
        }
    }
}
