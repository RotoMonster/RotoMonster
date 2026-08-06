#!/usr/bin/env python3
"""
Fixes CS0579 from patch_layout_league_v1.py.

Players.cshtml.cs was the only one of the four pages with [BindProperty] on its
OWN LINE above the property rather than inline with it. v1 removed the property
line and left the attribute dangling, where it attached itself to SearchTerm -
which already carries [BindProperty(SupportsGet = true)]. Hence "Duplicate
'BindProperty' attribute".

Run from the repo root:  python3 fix_players_bindproperty_v1.py
"""

import os
import sys

PATH = "RotoMonster.Web/Pages/Players.cshtml.cs"

OLD = """        [BindProperty]

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
"""

NEW = """        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
"""


def main():
    if not os.path.exists(PATH):
        print("Cannot find %s - run this from the repo root." % PATH)
        sys.exit(1)

    with open(PATH, "rb") as f:
        raw = f.read()
    bom = raw.startswith(b"\xef\xbb\xbf")
    if bom:
        raw = raw[3:]
    text = raw.decode("utf-8")
    crlf = text.count("\r\n")
    lf = text.count("\n") - crlf
    ending = "\r\n" if crlf > lf else "\n"
    text = text.replace("\r\n", "\n")

    n = text.count(OLD)
    if n != 1:
        print("  FAILED: anchor matched %d times, expected 1." % n)
        print("  If it matched 0, the file may already be fixed.")
        sys.exit(1)

    text = text.replace(OLD, NEW)
    print("  ok  removed the orphaned [BindProperty]")

    data = text.replace("\n", ending).encode("utf-8")
    if bom:
        data = b"\xef\xbb\xbf" + data
    with open(PATH, "wb") as f:
        f.write(data)
    print("  written (%s)" % ("CRLF" if ending == "\r\n" else "LF"))
    print("\nDone. Now: dotnet build")


if __name__ == "__main__":
    main()
