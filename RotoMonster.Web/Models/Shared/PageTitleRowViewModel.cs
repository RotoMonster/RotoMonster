using Microsoft.AspNetCore.Mvc.Rendering;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class PageTitleRowViewModel
    {
        public string Title { get; set; }

        public SelectList UserLeagueList { get; set; }
        public int SelectedUserLeagueId { get; set; }
        public string LeagueFieldName { get; set; } = "SelectedUserLeagueId";

        public bool ShowRefreshRosters { get; set; }
        public string RefreshHandler { get; set; } = "Refresh";

        public bool ShowPlayerSearch { get; set; }
        public string PlayerSearchId { get; set; } = "pageTitleRowSearch";
        public string PlayerSearchPlaceholder { get; set; } = "Search players";
    }
}
