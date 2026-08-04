#!/usr/bin/env python3
"""
Marks the Total MonsterBar call sites with scope "t".

Without this, a row rendering both a Game bar and a Total bar for the same
player generates the same tooltip ids twice, and getElementById resolves every
tooltip on that row to the first one.

Run AFTER replacing MonsterBarBadgeViewModel.cs, from the RotoMonster repo root:
    python3 patch_mbar_scope.py
"""

import io
import sys

EDITS = [
    ("RotoMonster/Pages/DepthCharts.cshtml",
     "MonsterBarBadgeViewModel.From(dp.MonsterBarTotalPlayer, Model.TotalMonsterBar.MonsterBarItems)",
     'MonsterBarBadgeViewModel.From(dp.MonsterBarTotalPlayer, Model.TotalMonsterBar.MonsterBarItems, false, null, "t")'),

    ("RotoMonster/Pages/Shared/_PlayerTable.cshtml",
     "MonsterBarBadgeViewModel.From(dp.MonsterBarTotalPlayer, Model.MonsterBarTotal.MonsterBarItems)",
     'MonsterBarBadgeViewModel.From(dp.MonsterBarTotalPlayer, Model.MonsterBarTotal.MonsterBarItems, false, null, "t")'),
]


def read(path):
    return io.open(path, "r", encoding="utf-8-sig", newline="").read()


def write(path, text):
    io.open(path, "w", encoding="utf-8-sig", newline="").write(text)


def main():
    contents = {}
    problems = []
    todo = []

    for path, old, new in EDITS:
        try:
            if path not in contents:
                contents[path] = read(path)
        except IOError:
            problems.append("cannot read %s - are you in the repo root?" % path)
            continue

        if new in contents[path]:
            print("note: already scoped in %s" % path)
            continue

        found = contents[path].count(old)
        if found != 1:
            problems.append("%s: expected 1 total-bar call, found %d" % (path, found))
        else:
            todo.append((path, old, new))

    if problems:
        print("ABORTED - nothing was written:")
        for p in problems:
            print("  " + p)
        return 1

    if not todo:
        print("Nothing to do - already patched.")
        return 0

    for path, old, new in todo:
        contents[path] = contents[path].replace(old, new)

    for path in set(p for p, _o, _n in todo):
        write(path, contents[path])
        print("patched %s" % path)

    return 0


if __name__ == "__main__":
    sys.exit(main())
