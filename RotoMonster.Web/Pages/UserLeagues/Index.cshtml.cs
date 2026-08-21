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
        }

        // The Import page now lists and manages leagues as well as adding
        // them, so this one exists only to keep old links working. Everything
        // that used to redirect here lands there instead.
        public IActionResult OnGet()
        {
            return RedirectToPage("./Import");
        }

        public IActionResult OnPost()
        {
            return RedirectToPage("./Import");
        }

    }

}
