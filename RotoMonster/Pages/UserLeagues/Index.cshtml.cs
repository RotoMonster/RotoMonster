using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using RotoMonster.Data;
using RotoMonster.Core.Libs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RotoMonster.Pages.UserLeagues
{
    [Authorize]
    public class IndexModel : RMPageModel
    {

        public IndexModel(IRMData db, IRMSharedData sharedDb, IConfiguration config, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
            SelectedUserLeagues = db.GetUserLeagues(UserId);
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            return RedirectToPage("./Index");
        }

    }

}
