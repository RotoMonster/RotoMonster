using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;

namespace RotoMonster.Pages
{
    public class ArticlesModel : RMPageModel
    {
        public IEnumerable<Article> Articles { get; set; }
        public IEnumerable<Article> AutomatedArticles { get; set; }
        public bool ShowingAutomated { get; set; }
        public Player Player { get; set; } = null;

        public ArticlesModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public void OnGet(bool auto = false, int articleid = 0, int playerid = 0)
        {
            InitGet("Articles");

            int pastDays = 3;
            if (playerid > 0)
                Player = (from p in db.GetPlayers() where p.Id == playerid select p).FirstOrDefault();
            if (Player != null)
                pastDays = 30;

            List<Article> tmpArticles = new List<Article>();
            if (articleid > 0)
            {
                var article = db.GetArticle(articleid);
                if (article != null)
                    tmpArticles.Add(article);
            }
            else
            {
                foreach (var article in db.GetArticles(DateTime.Today.AddDays(-1 * pastDays), DateTime.Today.AddDays(1), true))
                {
                    if (playerid == 0)
                        tmpArticles.Add(article);
                    else
                    {
                        if (article.ArticlePlayers.Find(p => p.PlayerId == playerid) != null)
                            tmpArticles.Add(article);
                    }
                }
            }

            AutomatedArticles = (from a in tmpArticles where a.IsAutomated select a).ToList();
            if (!auto)
                Articles = (from a in tmpArticles where !a.IsAutomated select a).ToList();
            else
                Articles = tmpArticles;
            ShowingAutomated = auto;
        }

        public IActionResult OnPost(string includeautomated)
        {
            return RedirectToPage("./Articles", new
            {
                auto = !string.IsNullOrEmpty(includeautomated)
            });
        }

    }
}
