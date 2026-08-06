#!/usr/bin/env python3
"""
Two fixes Ry raised: the dropdowns are orange on a baseball site, and the
league dropdown is a plain browser select.

ORANGE: bm.light.css sets --brand-accent to #ff8c42, which is Basketball
Monster's orange. bm.mlb.light.css overrides the whole brand block to baseball
green (--brand-primary #2d7a3e, --brand-accent #4ade80) plus MLB position
colours. RotoMonster never loaded the sport file - Ken's 7-29 chain
(bm.light -> bm.<sport>.light -> rm.light) with the middle link missing.
rm.light.css only READS the brand variables and defines none, so dropping the
sport file between them is safe.

PLAIN DROPDOWN: bm.light.css already styles .bm-custom-select (rounded
trigger, animated arrow, sliding panel, checkmark on the selected option) and
Scripts/custom-dropdown-update.js drives it. The native select stays in the
markup but is hidden by CSS; the JS mirrors the value into it and dispatches a
bubbling change event, so onchange="this.form.submit()" still fires.

NOT USING the library Dropdown component: it emits onchange="__doPostBack(...)"
which is WebForms and does not exist in Razor. The markup is written directly
instead, which also matches Ken's all-Razor decision for RotoMonster.

COPIES ONLY - nothing in ~/Desktop/RotoMonsterUI is modified, so Basketball and
Baseball Monster are untouched.

Run from the repo root:  python3 patch_menubar_style_v1.py
"""

import os
import shutil
import sys

LIB = os.path.expanduser("~/Desktop/RotoMonsterUI")
CSS_DIR = "RotoMonster.Web/wwwroot/css"
JS_DIR = "RotoMonster.Web/wwwroot/js"
LAYOUT = "RotoMonster.Web/Pages/Shared/_Layout.cshtml"

COPIES = [
    (os.path.join(LIB, "css", "bm.mlb.light.css"), os.path.join(CSS_DIR, "bm.mlb.light.css")),
    (os.path.join(LIB, "css", "bm.nba.light.css"), os.path.join(CSS_DIR, "bm.nba.light.css")),
    (os.path.join(LIB, "css", "bm.mlb.dark.css"), os.path.join(CSS_DIR, "bm.mlb.dark.css")),
    (os.path.join(LIB, "css", "bm.nba.dark.css"), os.path.join(CSS_DIR, "bm.nba.dark.css")),
    (os.path.join(LIB, "Scripts", "custom-dropdown-update.js"),
     os.path.join(JS_DIR, "custom-dropdown-update.js")),
]

# ------------------------------------------------------------ sport stylesheet

CSS_OLD = """    <link rel="stylesheet" href="~/css/bm.light.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/css/rm.light.css" asp-append-version="true" />
"""

CSS_NEW = """    <link rel="stylesheet" href="~/css/bm.light.css" asp-append-version="true" />
    @{
        // Declared here rather than reusing the `sport` variable below, which is
        // defined after </head>. Only MLB and NBA override files exist.
        string sportCss = ViewData["sport"].ToString().ToLower();
    }
    @if (sportCss == "mlb" || sportCss == "nba")
    {
        <link rel="stylesheet" href="~/css/bm.@(sportCss).light.css" asp-append-version="true" />
    }
    <link rel="stylesheet" href="~/css/rm.light.css" asp-append-version="true" />
"""

# ------------------------------------------------------------------- the script

JS_OLD = """    <script src="~/js/rotomonster-ui.js" asp-append-version="true"></script>
"""

JS_NEW = """    <script src="~/js/rotomonster-ui.js" asp-append-version="true"></script>
    <script src="~/js/custom-dropdown-update.js" asp-append-version="true"></script>
"""

# --------------------------------------------------------------- the dropdown

DD_OLD = """                            <select name="l" class="form-control" style="min-width: 170px;" onchange="this.form.submit()" title="League">
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
"""

DD_NEW = """                            @{
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
"""


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


def main():
    if not os.path.isdir(LIB):
        print("Cannot find the library at %s" % LIB)
        sys.exit(1)
    for p in [CSS_DIR, JS_DIR, LAYOUT]:
        if not os.path.exists(p):
            print("Cannot find %s - run this from the repo root." % p)
            sys.exit(1)
    for src, _ in COPIES:
        if not os.path.exists(src):
            print("Cannot find %s" % src)
            sys.exit(1)

    print("Copying from the library (read-only - nothing there is modified)")
    for src, dst in COPIES:
        shutil.copyfile(src, dst)
        print("  ok  %s" % dst)

    print("Patching %s" % LAYOUT)
    text, ending, bom = read(LAYOUT)

    if "bm-custom-select" in text:
        print("  FAILED: already patched")
        sys.exit(1)

    text = swap(text, CSS_OLD, CSS_NEW, "sport stylesheet link")
    text = swap(text, JS_OLD, JS_NEW, "custom dropdown script")
    text = swap(text, DD_OLD, DD_NEW, "modern league dropdown")

    if "@using System.Linq" not in text and "@using RotoMonster.Core" in text:
        text = text.replace("@using RotoMonster.Core\n",
                            "@using RotoMonster.Core\n@using System.Linq\n", 1)
        print("  ok  added @using System.Linq for FirstOrDefault")

    write(LAYOUT, text, ending, bom)
    print("  written (%s)" % ("CRLF" if ending == "\r\n" else "LF"))
    print("\nDone. Razor views recompile on reload - refresh the browser.")


if __name__ == "__main__":
    main()
