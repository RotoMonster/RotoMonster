using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using System.Collections.Generic;

namespace RotoMonster.Pages
{
    public class HelpModel : RMPageModel
    {
        public List<Helper> Helpers { get; set; }
        public List<PlayerType> PlayerTypes { get; set; }

        public HelpModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public IActionResult OnGet()
        {
            InitGet("Help");

            Helpers = db.GetHelpers();
            PlayerTypes = db.GetPlayerTypes();

            //DisplayMonsterBarPlayer = new DisplayMonsterBarPlayer();
            //DisplayMonsterBarPlayer.MonsterBarPlayer = new MonsterBarPlayer();
            //DisplayMonsterBarPlayer.MonsterBarPlayer.IsGoodFreeAgent = true;
            //DisplayMonsterBarPlayer.MonsterBarPlayer=db.GetPlayer()

            //var monsterBarItem = new MonsterBarItem();
            //monsterBarItem.Description = "Last Season";
            //monsterBarPlayer.Player

            return Page();
        }

    }
}
