#!/usr/bin/env python3
"""
The league dropdown renders but collapses to a sliver.

CAUSE: it sits in `.navbar-collapse.d-sm-inline-flex.flex-sm-row-reverse`
alongside `ul.navbar-nav.flex-grow-1`. The ul grows to take the free space and
the select, having no intrinsic minimum, gets shrunk to almost nothing.

FIX: flex-shrink-0 on the form so it stops giving up space, plus a min-width on
the select so it is wide enough to read a league title.

Run from the repo root:  python3 fix_league_dropdown_width_v1.py
"""

import os
import sys

PATH = "RotoMonster.Web/Pages/Shared/_Layout.cshtml"

FORM_OLD = """                        <form method="get" class="d-flex align-items-center me-3">
"""
FORM_NEW = """                        <form method="get" class="d-flex align-items-center me-3 flex-shrink-0">
"""

SELECT_OLD = """                            <select name="l" class="form-control" onchange="this.form.submit()" title="League">
"""
SELECT_NEW = """                            <select name="l" class="form-control" style="min-width: 170px;" onchange="this.form.submit()" title="League">
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

    if "min-width: 170px" in text:
        print("  FAILED: already patched")
        sys.exit(1)

    for old, new, label in [
        (FORM_OLD, FORM_NEW, "flex-shrink-0 on the league form"),
        (SELECT_OLD, SELECT_NEW, "min-width on the select"),
    ]:
        n = text.count(old)
        if n != 1:
            print("  FAILED on %s: anchor matched %d times, expected 1" % (label, n))
            sys.exit(1)
        text = text.replace(old, new)
        print("  ok  %s" % label)

    data = text.replace("\n", ending).encode("utf-8")
    if bom:
        data = b"\xef\xbb\xbf" + data
    with open(PATH, "wb") as f:
        f.write(data)
    print("  written (%s)" % ("CRLF" if ending == "\r\n" else "LF"))
    print("\nDone. Razor views recompile on reload - no rebuild needed.")


if __name__ == "__main__":
    main()
