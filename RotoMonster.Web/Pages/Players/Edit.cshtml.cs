using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RotoMonster.Core;
using RotoMonster.Data;

namespace RotoMonster
{
    public class EditModel : PageModel
    {
        private readonly IRMData playerData;

        [BindProperty]
        public Player Player { get; set; }

        public EditModel(IRMData playerData)
        {
            this.playerData = playerData;
        }

        public ActionResult OnGet(int? playerId)
        {
            if (playerId.HasValue)
            {
                Player = playerData.GetById(playerId.Value);
            }
            else
            {
                Player = new Player();

            }
            if (Player == null)
                return RedirectToPage("./NotFound");

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (Player.Id > 0)
            {
                playerData.Update(Player);
            }
            else
            {
                playerData.Add(Player);
            }
            playerData.Commit();

            TempData["Message"] = "The player was saved.";

            return RedirectToPage("./Detail", new { playerId = Player.Id });
        }
    }
}