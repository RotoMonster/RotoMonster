#!/usr/bin/env python3
"""
Ken's Menu Bar doc: move the league dropdown into _Layout.

WHY IT WORKS WITH NO PAGE-HANDLER CHANGES: every page already accepts the
league as `?l=` on GET - DepthCharts, Scores, Players and PlayerRankings all
take `int? l` and do `SelectedUserLeagueId = l.GetValueOrDefault()`. So a plain
GET form that sets `l` navigates correctly on every page already.

WHAT STILL NEEDED CHANGING: the layout has to know which league the page
actually RESOLVED, so it can mark the right option selected. That value lives
in a per-page `SelectedUserLeagueId`. This hoists it onto RMPageModel and
deletes the four shadowing declarations, so the layout can read one property.

WHY NOT A POST: a layout-level <form method="post"> would wrap the whole body
and nest inside the page forms DepthCharts and PlayerRankings already have,
which is invalid HTML. A GET form with no asp-page submits to the current URL.

QUERY STRING NOTE: a bare GET form replaces the whole query string, so hidden
inputs re-emit the params worth keeping (playerId, date, t, hid, g, top,
SearchTerm) when they are present on the current request.

Run from the repo root:  python3 patch_layout_league_v1.py
"""

import os
import sys

RMPAGE = "RotoMonster.Web/Pages/RMPageModel.cs"
LAYOUT = "RotoMonster.Web/Pages/Shared/_Layout.cshtml"
DEPTH_VIEW = "RotoMonster.Web/Pages/DepthCharts.cshtml"
PAGES = [
    "RotoMonster.Web/Pages/DepthCharts.cshtml.cs",
    "RotoMonster.Web/Pages/Scores.cshtml.cs",
    "RotoMonster.Web/Pages/Players.cshtml.cs",
    "RotoMonster.Web/Pages/PlayerRankings.cshtml.cs",
]


def read(path):
    with open(path, "rb") as f:
        raw = f.read()
    bom = raw.startswith(b"\xef\xbb\xbf")
    if bom:
        raw = raw[3:]
    text = raw.decode("utf-8")
    crlf = text.count("\r\n")
    lf = text.count("\n") - crlf
    return text.replace("\r\n", "\n"), ("\r\n" if crlf > lf else "\n"), bom


def write(path, text, ending, bom):
    data = text.replace("\n", ending).encode("utf-8")
    if bom:
        data = b"\xef\xbb\xbf" + data
    with open(path, "wb") as f:
        f.write(data)


def swap(text, old, new, label):
    n = text.count(old)
    if n != 1:
        print("  FAILED on %s: anchor matched %d times, expected 1" % (label, n))
        sys.exit(1)
    print("  ok  %s" % label)
    return text.replace(old, new)


# ---------------------------------------------------------------- RMPageModel

RM_OLD = """        public List<UserLeague> SelectedUserLeagues { get; set; }
"""

RM_NEW = """        public List<UserLeague> SelectedUserLeagues { get; set; }

        // Hoisted from the individual pages so _Layout can render one league
        // dropdown for the whole site. Pages still assign it exactly as before.
        [BindProperty]
        public int SelectedUserLeagueId { get; set; }
"""

# --------------------------------------------------------- per-page shadowing

PAGE_DECLS = {
    "RotoMonster.Web/Pages/DepthCharts.cshtml.cs":
        "        [BindProperty] public int SelectedUserLeagueId { get; set; }\n",
    "RotoMonster.Web/Pages/Scores.cshtml.cs":
        "        [BindProperty] public int SelectedUserLeagueId { get; set; }\n",
    "RotoMonster.Web/Pages/PlayerRankings.cshtml.cs":
        "        [BindProperty] public int SelectedUserLeagueId { get; set; }\n",
    "RotoMonster.Web/Pages/Players.cshtml.cs":
        "        public int SelectedUserLeagueId { get; set; }\n",
}

# --------------------------------------------------------------------- layout

LAYOUT_ANCHOR = """                    <form method="get" asp-page="/Players" class="d-flex align-items-center me-3">
                        <input type="search" id="navPlayerSearch" name="SearchTerm" class="form-control"
                               placeholder="Search players" autocomplete="off" />
                    </form>
"""

LAYOUT_NEW = """                    <form method="get" asp-page="/Players" class="d-flex align-items-center me-3">
                        <input type="search" id="navPlayerSearch" name="SearchTerm" class="form-control"
                               placeholder="Search players" autocomplete="off" />
                    </form>
                    @{
                        // RMPageModel-derived pages only. Identity pages derive
                        // straight from PageModel, so this is null there and the
                        // dropdown simply does not render.
                        var rmModel = ViewContext.ViewData.Model as RotoMonster.Pages.RMPageModel;
                    }
                    @if (rmModel != null && rmModel.SelectedUserLeagues != null && rmModel.SelectedUserLeagues.Count > 0)
                    {
                        <form method="get" class="d-flex align-items-center me-3">
                            @* A bare GET form replaces the whole query string, so re-emit
                               the params the current page cares about. *@
                            @foreach (var keep in new[] { "playerId", "date", "t", "hid", "g", "top", "SearchTerm" })
                            {
                                if (Context.Request.Query.ContainsKey(keep))
                                {
                                    <input type="hidden" name="@keep" value="@Context.Request.Query[keep]" />
                                }
                            }
                            <select name="l" class="form-control" onchange="this.form.submit()" title="League">
                                @foreach (var userLeague in rmModel.SelectedUserLeagues)
                                {
                                    if (userLeague.Id == rmModel.SelectedUserLeagueId)
                                    {
                                        <option value="@userLeague.Id" selected>@userLeague.ListDisplayTitle</option>
                                    }
                                    else
                                    {
                                        <option value="@userLeague.Id">@userLeague.ListDisplayTitle</option>
                                    }
                                }
                            </select>
                        </form>
                    }
"""

# ------------------------------------------------------- DepthCharts call site

DEPTH_OLD = """            Title = "Depth Charts",
            UserLeagueList = (Microsoft.AspNetCore.Mvc.Rendering.SelectList)ViewBag.UserLeagueList,
            SelectedUserLeagueId = Model.SelectedUserLeagueId
"""

DEPTH_NEW = """            Title = "Depth Charts",
            // League dropdown now lives in _Layout for the whole site. Leaving
            // UserLeagueList unset makes _PageTitleRow skip its own copy.
            SelectedUserLeagueId = Model.SelectedUserLeagueId
"""


def main():
    for p in [RMPAGE, LAYOUT, DEPTH_VIEW] + PAGES:
        if not os.path.exists(p):
            print("Cannot find %s - run this from the repo root." % p)
            sys.exit(1)

    print("Patching %s" % RMPAGE)
    text, ending, bom = read(RMPAGE)
    if "public int SelectedUserLeagueId" in text:
        print("  FAILED: already patched")
        sys.exit(1)
    text = swap(text, RM_OLD, RM_NEW, "add SelectedUserLeagueId to RMPageModel")
    if "using Microsoft.AspNetCore.Mvc;" not in text:
        print("  FAILED: RMPageModel.cs has no 'using Microsoft.AspNetCore.Mvc;'"
              " - [BindProperty] would not resolve. Add it and re-run.")
        sys.exit(1)
    print("  ok  BindProperty using present")
    write(RMPAGE, text, ending, bom)
    print("  written")

    for path in PAGES:
        print("Patching %s" % path)
        text, ending, bom = read(path)
        text = swap(text, PAGE_DECLS[path], "", "remove shadowing declaration")
        write(path, text, ending, bom)
        print("  written")

    print("Patching %s" % LAYOUT)
    text, ending, bom = read(LAYOUT)
    if 'name="l"' in text:
        print("  FAILED: already patched")
        sys.exit(1)
    text = swap(text, LAYOUT_ANCHOR, LAYOUT_NEW, "insert league dropdown")
    write(LAYOUT, text, ending, bom)
    print("  written")

    print("Patching %s" % DEPTH_VIEW)
    text, ending, bom = read(DEPTH_VIEW)
    text = swap(text, DEPTH_OLD, DEPTH_NEW, "drop the duplicate dropdown")
    write(DEPTH_VIEW, text, ending, bom)
    print("  written")

    print("\nDone. Now: dotnet build")


if __name__ == "__main__":
    main()
