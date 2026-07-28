using Microsoft.AspNetCore.Mvc;
using RotoMonster.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RotoMonster.ViewComponents
{
    public class PlayerCountViewComponent
        : ViewComponent
    {
        private readonly IRMData playerData;

        public PlayerCountViewComponent(IRMData playerData)
        {
            this.playerData = playerData;
        }

        public IViewComponentResult Invoke()
        {
            var count = playerData.GetCountOfPlayers();

            return View(count);
        }
    }
}
