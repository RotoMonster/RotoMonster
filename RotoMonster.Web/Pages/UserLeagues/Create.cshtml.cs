using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RotoMonster.Core;
using RotoMonster.Data;
using RotoMonster.Core.Libs;

namespace RotoMonster.Pages.UserLeagues
{
    public class CreateModel : PageModel
    {
        private readonly RotoMonster.Data.RMDBContext _context;

        public CreateModel(RotoMonster.Data.RMDBContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["FantasyProviderId"] = new SelectList(_context.FantasyProviders, "Id", "Name");

            UserLeagueLib userLeagueLib = new UserLeagueLib();
            ViewData["ScoringSystemList"] = userLeagueLib.ScoringSystemList;
            ViewData["LeagueTypeList"] = userLeagueLib.LeagueTypeList;
            ViewData["LineupFrequencyList"] = userLeagueLib.LineupFrequencyList;

            return Page();
        }

        [BindProperty]
        public UserLeague UserLeague { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.UserLeagues.Add(UserLeague);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
