#!/usr/bin/env python3
"""
Ken's "Some RotoMonster UI" doc.

  1. MonsterBar column titles show the season year instead of LS / S.
  2. Space between the injury abbreviation and the games count on the status badge.

Item 2 of his doc (the probable-pitcher question mark) is a library change
plus a data change, so it is not in here.

Run from the repo root:  python3 patch_rm_ui_v1.py
"""

import io
import os
import sys

ROOT = os.path.dirname(os.path.abspath(__file__))


def read(path):
    with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
        return f.read()


def write(path, text, crlf):
    text = text.replace("\r\n", "\n")
    if crlf:
        text = text.replace("\n", "\r\n")
    with io.open(path, "w", encoding="utf-8-sig" if crlf else "utf-8", newline="") as f:
        f.write(text)


def patch(path, edits, crlf):
    full = os.path.join(ROOT, path)
    if not os.path.exists(full):
        sys.exit("MISSING: " + path)

    text = read(full)
    flat = text.replace("\r\n", "\n")

    for name, old, new in edits:
        if new in flat and old not in flat:
            print("  skip (already applied): " + name)
            continue
        count = flat.count(old)
        if count != 1:
            sys.exit("ANCHOR MATCHED %d TIMES in %s: %s" % (count, path, name))
        flat = flat.replace(old, new)
        print("  ok: " + name)

    write(full, flat, crlf)


# ---------------------------------------------------------------- item 1
DATA = "RotoMonster.Data/RMSqlData.cs"

old_title = """                item.Title = id;
                item.DisplayOrder = monsterBar.MonsterBarItems.Count + 1;"""

new_title = """                switch (id)
                {
                    case "LS":
                        item.Title = processSeason != null ? MonsterBarYearLabel(processSeason) : id;
                        break;
                    case "S":
                        item.Title = MonsterBarYearLabel(season);
                        break;
                    default:
                        item.Title = id;
                        break;
                }
                item.DisplayOrder = monsterBar.MonsterBarItems.Count + 1;"""

# helper goes in immediately above GetMonsterBar's enclosing region; anchor on
# the cache check at the top of the method so it lands somewhere stable.
old_helper_anchor = """            if (CacheItemExists(cacheId))
                return (MonsterBar)GetCacheItem(cacheId);

            MonsterBar monsterBar = new MonsterBar();"""

new_helper_anchor = """            if (CacheItemExists(cacheId))
                return (MonsterBar)GetCacheItem(cacheId);

            MonsterBar monsterBar = new MonsterBar();"""

helper = """
        private string MonsterBarYearLabel(Season season)
        {
            if (season == null)
                return "";

            int year = season.Year.GetValueOrDefault(season.StartDate.Year);
            return "'" + (year % 100).ToString("00");
        }
"""

# ---------------------------------------------------------------- item 3
STATUS = "RotoMonster.Web/Pages/Shared/_PlayerStatus.cshtml"

old_gap = """            string g = Model.EstimatedGamesToMiss.Count.ToString() + "g";
            <small>@g</small>"""

new_gap = """            string g = Model.EstimatedGamesToMiss.Count.ToString() + "g";
            <small class="status-badge-count">@g</small>"""

CSS = "RotoMonster.Web/wwwroot/css/rm.light.css"

css_block = """
/* Space between the injury abbreviation and the games count. Sits in
   rm.light.css because it is spacing, not colour, so it applies in both themes. */
.badge .status-badge-count {
    margin-left: 3px;
}
"""


def main():
    print(DATA)
    patch(DATA, [("MonsterBar year titles", old_title, new_title)], crlf=True)

    # append the helper inside the class, just before the final closing braces
    full = os.path.join(ROOT, DATA)
    text = read(full).replace("\r\n", "\n")
    if "MonsterBarYearLabel(Season season)" not in text:
        idx = text.rstrip().rfind("\n    }")
        if idx == -1:
            sys.exit("could not find the class closing brace in " + DATA)
        text = text[:idx] + "\n" + helper + text[idx:]
        write(full, text, True)
        print("  ok: MonsterBarYearLabel helper added")
    else:
        print("  skip (already applied): MonsterBarYearLabel helper")

    print(STATUS)
    patch(STATUS, [("injury badge games class", old_gap, new_gap)], crlf=False)

    print(CSS)
    full = os.path.join(ROOT, CSS)
    text = read(full).replace("\r\n", "\n")
    if ".status-badge-count" in text:
        print("  skip (already applied): status-badge-count rule")
    else:
        write(full, text.rstrip("\n") + "\n" + css_block, False)
        print("  ok: status-badge-count rule appended")

    print("\ndone")


if __name__ == "__main__":
    main()
