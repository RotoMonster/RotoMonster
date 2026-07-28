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
    public class DeleteModel : PageModel
    {
        private readonly IRMData playerData;
        public Player Player { get; set; }

        public DeleteModel(IRMData playerData)
        {
            this.playerData = playerData;
        }

        public IActionResult OnGet(int playerId)
        {
            Player = playerData.GetById(playerId);
            if (Player == null)
                return RedirectToPage("./NotFound");

            return Page();
        }

        public IActionResult OnPost(int playerId)
        {
            var player = playerData.Delete(playerId);
            playerData.Commit();

            if(player==null)
                return RedirectToPage("./NotFound");

            TempData["Message"] = player.FirstName + " " + player.LastName + " deleted";
            return RedirectToPage("./List");
        }

    }
}