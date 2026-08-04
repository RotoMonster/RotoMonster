using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using RotoMonster.Data;

namespace RotoMonster.Pages.UserLeagues
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IRMData db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor contextAccessor;

        public UserLeague UserLeague { get; set; }
        public string UserId { get; set; }

        public DeleteModel(IRMData db, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
        {
            this.db = db;
            this.userManager = userManager;
            this.contextAccessor = contextAccessor;
            UserId = userManager.GetUserId(contextAccessor.HttpContext.User);
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userLeagues = await db.GetUserLeaguesAsync(UserId);
            UserLeague = (from u in userLeagues where u.Id == id select u).FirstOrDefault();
            if (UserLeague == null)
                return RedirectToPage("./NotFound");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {

            await db.DeleteUserLeagueAsync(id);
            await db.CommitAsync();

            TempData["Message"] = "League deleted";
            return RedirectToPage("./Index");
        }

    }
}

