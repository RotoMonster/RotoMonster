#!/usr/bin/env python3
"""
Ken's RotoMonster Menu Bar doc: organize the header like Basketball Monster.

  Row 1: the menu plus user login/logout, with the other sports' icons there
         too, "smaller and optional for now".
  Row 2: the Logo along with the Search and Season Progress.

Season Progress stays TEXT, not the library's SeasonProgress bar - Ken's own
mockup shows "Season: 35% Complete" as text, and ViewData["SeasonState"] holds
Season.State, not a percentage. So this patch is purely structural.

WHAT MOVES
  down to row 2 : _Logo, the player search form, the league dropdown, Season
  stays in row 1: Tools / Settings / Helpers, mail, twitter, sport icons,
                  the admin gear, and _LoginPartial

WHY THE HEADER WRAPPED TO THREE LINES: everything was in one nav, and the
Season text was jammed into a nav <li> with no room. Splitting the rows is what
fixes the alignment, not a CSS tweak.

OTHER CHANGES
  - sport icons drop from Size 30 to Size 22 (Ken: "smaller")
  - flex-sm-row-reverse is gone; row 1 now reads left-to-right with login
    pushed right by ms-auto, matching the mockup
  - mb-3 moves from the nav to the row 2 wrapper so page spacing is unchanged

The whole <header> block is replaced by regex rather than an exact text match,
so trailing whitespace inside it cannot break the patch.

Run from the repo root:  python3 patch_menubar_tworow_v1.py
"""

import os
import re
import sys

LAYOUT = "RotoMonster.Web/Pages/Shared/_Layout.cshtml"

NEW_HEADER = '''    <header>

        @* ROW 1 - menu and account. *@
        <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light light py-1">
            <div class="container">
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse" aria-controls="navbarSupportedContent"
                        aria-expanded="false" aria-label="Toggle navigation">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="navbar-collapse collapse d-sm-inline-flex align-items-center w-100">
                    <ul class="navbar-nav align-items-center">

                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                                Tools
                            </a>
                            <div class="dropdown-menu">
                                <a class="dropdown-item" asp-area="" asp-page="/PlayerRankings">Rankings</a>
                                <a class="dropdown-item" asp-area="" asp-page="/Scores">Box Scores</a>
                                <a class="dropdown-item" asp-area="" asp-page="/DepthCharts">Depth Charts</a>
                            </div>
                        </li>

                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle" href="#" id="navbarDropdown" role="button" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                Settings
                            </a>
                            <div class="dropdown-menu" aria-labelledby="navbarDropdown">
                                <a class="dropdown-item text-dark" asp-area="" asp-page="/UserLeagues/Index">Leagues</a>
                                <a class="dropdown-item text-dark" asp-area="" asp-page="/DisplaySettings">Display</a>
                            </div>
                        </li>

                        @if (ViewData["Helpers"] != null)
                        {
                            <li class="nav-item dropdown">
                                <a class="nav-link dropdown-toggle" href="#" id="navbarDropdown" role="button" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    Helpers
                                </a>
                                <div class="dropdown-menu" aria-labelledby="navbarDropdown">
                                    @foreach (Helper helper in (List<Helper>)ViewData["Helpers"])
                                    {
                                        <a href="@Html.Raw(helper.GetPlayerTypeUrl(helper.Url,null))" class="dropdown-item text-dark">@Html.Raw(helper.GetDisplayText(helper.Title,null))</a>
                                    }
                                    <div class="dropdown-divider"></div>
                                    <a class="dropdown-item text-dark" asp-area="" asp-page="/Help">Help Page</a>
                                </div>
                            </li>
                        }

                        <li class="nav-item">
                            <a class="nav-link text-dark" title="email support@rotomonster.com" href="mailto:support@rotomonster.com">@Html.Raw(new Icon(new IconInput { Type = IconType.Envelope, Size = 20 }).Render())</a>
                        </li>

                        <li class="nav-item me-2">
                            @{
                                string t = "https://twitter.com/rotomonster";
                            }
                            <a class="nav-link text-dark" target="_blank" href="@t">@Html.Raw(new Icon(new IconInput { Type = IconType.Twitter, Size = 20 }).Render())</a>
                        </li>

                        @* Ken: the other sports can be here too, smaller and optional for now. *@
                        @for (int i = 0; i < codes.Count(); i++)
                        {
                            string code = codes[i];
                            IconType icon = icons[i];
                            string cssCode = (sport == code) ? code.ToLower() + " text-white rounded" : code.ToLower() + "-txt";

                            <li class="nav-item">
                                <a class="nav-link p-0 ms-2" title="@code" href="/@code.ToLower()"><span class="p-1 @cssCode" style="display:inline-flex;align-items:center;">@Html.Raw(new Icon(new IconInput { Type = icon, Size = 22 }).Render())</span></a>
                            </li>
                        }

                        @if (User.IsInRole("Admin"))
                        {
                            <li class="nav-item ms-3">
                                <a class="nav-link d-flex align-items-center" asp-area="" asp-page="/Admin">
                                    @Html.Raw(new Icon(new IconInput { Type = IconType.Settings, Size = 16 }).Render())
                                    @if (ViewData["AdminErrors"] != null)
                                    {
                                        var logItems = (List<LogItem>)ViewData["AdminErrors"];
                                        <small class="ms-1">@logItems.Count</small>
                                    }
                                </a>
                            </li>
                        }

                    </ul>

                    <div class="ms-auto d-flex align-items-center">
                        <partial name="_LoginPartial" />
                    </div>
                </div>
            </div>
        </nav>

        @* ROW 2 - identity and the controls that apply to whatever page you are on. *@
        <div class="border-bottom box-shadow mb-3 py-2">
            <div class="container d-flex align-items-center flex-wrap gap-3">

                <partial name="_Logo" model='new RotoMonster.Models.Shared.LogoModel(){Sport=sport,Controller="/Index"}' />

                @{
                    // RMPageModel-derived pages only. Identity pages derive
                    // straight from PageModel, so this is null there and the
                    // dropdown simply does not render.
                    var rmModel = ViewContext.ViewData.Model as RotoMonster.Pages.RMPageModel;
                }
                @if (rmModel != null && rmModel.SelectedUserLeagues != null && rmModel.SelectedUserLeagues.Count > 0)
                {
                    <form method="get" class="d-flex align-items-center flex-shrink-0 mb-0">
                        @* A bare GET form replaces the whole query string, so re-emit
                           the params the current page cares about. *@
                        @foreach (var keep in new[] { "playerId", "date", "t", "hid", "g", "top", "SearchTerm" })
                        {
                            if (Context.Request.Query.ContainsKey(keep))
                            {
                                <input type="hidden" name="@keep" value="@Context.Request.Query[keep]" />
                            }
                        }
                        @{
                            var currentLeague = rmModel.SelectedUserLeagues
                                .FirstOrDefault(x => x.Id == rmModel.SelectedUserLeagueId);
                            var currentTitle = currentLeague != null
                                ? currentLeague.ListDisplayTitle
                                : rmModel.SelectedUserLeagues[0].ListDisplayTitle;
                        }
                        @* The visible control is the div; the select below it is hidden by
                           bm.light.css and kept only so the form has something to submit.
                           custom-dropdown-update.js mirrors the choice into it and fires a
                           bubbling change, which triggers the onchange. *@
                        <div class="bm-custom-select" data-name="l">
                            <div class="bm-custom-select-trigger">
                                <span class="bm-custom-select-value">@currentTitle</span>
                                <span class="bm-custom-select-arrow"></span>
                            </div>
                            <div class="bm-custom-select-options">
                                @foreach (var userLeague in rmModel.SelectedUserLeagues)
                                {
                                    var optionClass = userLeague.Id == rmModel.SelectedUserLeagueId
                                        ? "bm-custom-select-option selected"
                                        : "bm-custom-select-option";
                                    <div class="@optionClass" data-value="@userLeague.Id">@userLeague.ListDisplayTitle</div>
                                }
                            </div>
                            <select name="l" onchange="this.form.submit()">
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
                        </div>
                    </form>
                }

                <form method="get" asp-page="/Players" class="d-flex align-items-center mb-0">
                    <input type="search" id="navPlayerSearch" name="SearchTerm" class="form-control"
                           placeholder="Search players" autocomplete="off" />
                </form>

                @if (ViewData["SeasonState"] != null)
                {
                    <div class="ms-auto small text-muted text-nowrap">
                        Season: @ViewData["SeasonState"]
                    </div>
                }

            </div>
        </div>

    </header>'''


def main():
    if not os.path.exists(LAYOUT):
        print("Cannot find %s - run this from the repo root." % LAYOUT)
        sys.exit(1)

    with open(LAYOUT, "rb") as f:
        raw = f.read()
    bom = raw.startswith(b"\xef\xbb\xbf")
    if bom:
        raw = raw[3:]
    text = raw.decode("utf-8")
    crlf = text.count("\r\n")
    lf = text.count("\n") - crlf
    ending = "\r\n" if crlf > lf else "\n"
    text = text.replace("\r\n", "\n")

    if "ROW 1 - menu and account" in text:
        print("  FAILED: already patched")
        sys.exit(1)

    # Sanity: the earlier patches must already be in, or this would silently
    # drop the league dropdown that patch 1 and 2a added.
    for needed, why in [
        ("bm-custom-select", "patch_menubar_style_v1 (modern dropdown)"),
        ('data-name="l"', "patch_layout_league_v1 (league dropdown)"),
    ]:
        if needed not in text:
            print("  FAILED: %s not found - run %s first" % (needed, why))
            sys.exit(1)
    print("  ok  earlier patches present")

    pattern = re.compile(r"^    <header>.*?^    </header>", re.DOTALL | re.MULTILINE)
    matches = pattern.findall(text)
    if len(matches) != 1:
        print("  FAILED: found %d <header> blocks, expected 1" % len(matches))
        sys.exit(1)
    print("  ok  located the header block (%d lines)" % matches[0].count("\n"))

    text = pattern.sub(lambda m: NEW_HEADER, text, count=1)

    data = text.replace("\n", ending).encode("utf-8")
    if bom:
        data = b"\xef\xbb\xbf" + data
    with open(LAYOUT, "wb") as f:
        f.write(data)
    print("  written (%s)" % ("CRLF" if ending == "\r\n" else "LF"))
    print("\nDone. Razor views recompile on reload - refresh the browser.")


if __name__ == "__main__":
    main()
