using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using RotoMonsterExternalAPIs.Client.Models.Support;

namespace RotoMonster.Pages.Support
{
    public class StatusModel : RMPageModel
    {
        private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

        private readonly SupportService _support;
        private readonly ILogger<PageModel> _log;

        public StatusModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor, ILogger<PageModel> logger, SupportService support)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
            _support = support;
            _log = logger;
        }

        public SupportTicketView View { get; set; }

        [BindProperty, Required, StringLength(5000, MinimumLength = 2)]
        public string Reply { get; set; }

        public bool JustCreated { get; set; }

        public bool JustReplied { get; set; }

        public bool Emailed { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string token, bool created = false, bool replied = false, bool emailed = false)
        {
            InitGet("Support Request");
            View = await _support.GetAsync(token);
            if (View == null) return NotFound();

            JustCreated = created;
            JustReplied = replied;
            Emailed = emailed;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string token)
        {
            InitGet("Support Request");

            if (!ModelState.IsValid)
            {
                View = await _support.GetAsync(token);
                if (View == null) return NotFound();
                return Page();
            }

            var result = await _support.ReplyAsync(token, Reply);
            if (!result.Success)
            {
                _log.LogWarning("Support reply failed: {Error}", result.ErrorMessage);
                View = await _support.GetAsync(token);
                if (View == null) return NotFound();
                ErrorMessage = "We couldn't send your reply. Please try again.";
                return Page();
            }

            return RedirectToPage("/Support/Status", new { token, replied = true });
        }

        public static string Render(SupportMessage message)
        {
            if (message.IsHtml)
            {
                var html = Sanitizer.Sanitize(message.Content ?? "");
                var quote = QuoteStart.Match(html);
                if (quote.Success && quote.Index > 0) html = Sanitizer.Sanitize(html.Substring(0, quote.Index));
                return html;
            }
            return WebUtility.HtmlEncode(message.Content ?? "").Replace("\n", "<br>");
        }

        private static readonly System.Text.RegularExpressions.Regex QuoteStart =
            new System.Text.RegularExpressions.Regex(@"-{3,}(?=[\s\S]{0,300}?wrote)|<blockquote",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Remove("img");
            sanitizer.AllowedTags.Remove("form");
            sanitizer.AllowedTags.Remove("input");
            sanitizer.AllowedTags.Remove("button");
            sanitizer.AllowedAttributes.Remove("style");
            sanitizer.AllowedAttributes.Remove("class");
            sanitizer.PostProcessNode += (s, e) =>
            {
                if (e.Node is AngleSharp.Html.Dom.IHtmlAnchorElement a)
                {
                    a.SetAttribute("target", "_blank");
                    a.SetAttribute("rel", "noopener noreferrer nofollow");
                }
            };
            return sanitizer;
        }
    }
}
