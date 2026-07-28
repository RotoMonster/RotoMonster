using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;

namespace RotoMonster
{
    public class ListModel : PageModel
    {
        private readonly IConfiguration config;
        private readonly IRMData playerData;
        private readonly RMDBContext db;
        private readonly ILogger<ListModel> logger;

        public string Message { get; set; }
        public IEnumerable<Player> Players { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public ListModel(IConfiguration config, IRMData playerData, RMDBContext db, ILogger<ListModel> logger)
        {
            this.config = config;
            this.playerData = playerData;
            this.db = db;
            this.logger = logger;
        }

        public void OnGet()
        {
            ViewData["PlayerCount"] = playerData.GetCountOfPlayers();
            logger.LogError("Executing listmodel");
            Players = playerData.GetPlayerByName(SearchTerm);
        }

    }
}
