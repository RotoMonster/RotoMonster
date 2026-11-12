(function () {
    function num(el) {
        if (!el) return null;
        var t = el.textContent;
        var ch = el.querySelector('.rm-badge-change');
        if (ch) t = t.replace(ch.textContent, '');
        t = t.replace(/[,%\s]/g, '');
        if (t === '' || t === '-' || t === '–') return null;
        var n = parseFloat(t);
        return isNaN(n) ? null : n;
    }
    function alpha(v, max) {
        return Math.round(8 + 24 * Math.min(1, max > 0 ? Math.abs(v) / max : 0)) + '%';
    }
    function pin(table) {
        var head = table.querySelectorAll('thead tr:first-child > .rm-pt-pin');
        var lefts = [], x = 0;
        head.forEach(function (th) { lefts.push(x); x += th.getBoundingClientRect().width; });
        var narrow = window.innerWidth < 768;
        table.querySelectorAll('tr').forEach(function (tr) {
            var cells = tr.querySelectorAll(':scope > .rm-pt-pin');
            cells.forEach(function (c, i) {
                var last = i === cells.length - 1;
                var off = narrow && !last;
                c.classList.toggle('rm-pt-unpin', off);
                c.style.left = off ? '' : (narrow ? 0 : (lefts[i] || 0)) + 'px';
                c.classList.toggle('rm-pt-pin-last', last);
            });
        });
    }
    function groups(table) {
        var old = table.querySelector('thead tr.rm-pt-groups');
        if (old) old.remove();
        var head = table.tHead.rows[0];
        var map = { ptd: 'Passing', patt: 'Passing', pyrd: 'Passing', pairy: 'Passing', int: 'Passing', comp: 'Passing', poort: 'Passing', sack: 'Passing', rtd: 'Rushing', ratt: 'Rushing', ryrd: 'Rushing', ryac: 'Rushing', rtbrk: 'Rushing', fumtd: 'Rushing', rxtd: 'Receiving', rxyrd: 'Receiving', rxairy: 'Receiving', rec: 'Receiving', targ: 'Receiving', rxrzt: 'Receiving' };
        var cells = Array.prototype.filter.call(head.cells, function (c) { return !c.classList.contains('rm-pt-hide') && !c.classList.contains('rm-pt-off'); });
        var known = false;
        var specs = cells.map(function (th) {
            var kind = th.getAttribute('data-pt');
            if (kind !== 'stat' && kind !== 'cval') return null;
            var btn = th.querySelector('button');
            var ab = (btn ? btn.textContent : th.textContent).trim().toLowerCase().replace(/\s+v$/, '');
            if (map[ab]) known = true;
            return { g: map[ab] || 'Other', v: kind === 'cval' };
        });
        if (!specs.some(Boolean)) return;
        var tr = document.createElement('tr');
        tr.className = 'rm-pt-groups';
        var i = 0, first = true;
        while (i < cells.length) {
            var s = specs[i], th = document.createElement('th');
            if (!s) {
                if (cells[i].classList.contains('rm-pt-pin')) th.className = 'rm-pt-pin';
                tr.appendChild(th);
                i++;
                continue;
            }
            var n = 0;
            while (i + n < cells.length && specs[i + n] && specs[i + n].v === s.v && (!known || specs[i + n].g === s.g)) n++;
            th.colSpan = n;
            th.className = 'rm-pt-grp' + (first ? ' rm-pt-grp-first' : '');
            th.innerHTML = '<span>' + (known ? s.g : 'Stats') + (s.v ? ' value' : '') + '</span>';
            tr.appendChild(th);
            first = false;
            i += n;
        }
        head.parentNode.insertBefore(tr, head);
        table.style.setProperty('--rm-pt-h1', tr.getBoundingClientRect().height + 'px');
    }
    function lastCol(table) {
        var headRow = table.tHead.querySelector('tr:not(.rm-pt-groups)');
        var last = -1;
        Array.prototype.forEach.call(headRow.cells, function (c, i) {
            if (!c.classList.contains('rm-pt-hide') && !c.classList.contains('rm-pt-off')) last = i;
        });
        table.querySelectorAll('.rm-pt-lastcol').forEach(function (c) { c.classList.remove('rm-pt-lastcol'); });
        table.querySelectorAll('tbody > tr').forEach(function (r) { if (r.cells[last]) r.cells[last].classList.add('rm-pt-lastcol'); });
        if (headRow.cells[last]) headRow.cells[last].classList.add('rm-pt-lastcol');
        var g = table.tHead.querySelector('tr.rm-pt-groups');
        if (g && g.lastElementChild) g.lastElementChild.classList.add('rm-pt-lastcol');
    }
    function hideCol(th, cells) {
        th.classList.add('rm-pt-hide');
        cells.forEach(function (c) { c.classList.add('rm-pt-hide'); });
    }
    function modern(table) {
        if (!table || !table.tHead) return;
        var oldGroups = table.querySelector('thead tr.rm-pt-groups');
        if (oldGroups) oldGroups.remove();
        var headRow = table.tHead.querySelector('tr:not(.rm-pt-groups)');
        var heads = Array.prototype.slice.call(headRow.cells);
        var rows = Array.prototype.slice.call(table.querySelectorAll('tbody > tr'));
        table.querySelectorAll('tbody .rm-badge, tbody .pct-badge, tbody .value-badge, tbody .rm-badge-change').forEach(function (b) { if (b.dataset.bg === undefined) b.dataset.bg = b.style.backgroundColor; b.style.backgroundColor = ''; });
        heads.forEach(function (th, i) {
            var kind = th.getAttribute('data-pt');
            if (!kind) return;
            var cells = rows.map(function (r) { return r.cells[i]; }).filter(Boolean);
            var vals = cells.map(function (c) { return num(c.querySelector('.pct-badge, .value-badge, .rm-badge') || c); });
            if (kind === 'extra') {
                cells.forEach(function (c) { c.style.backgroundColor = ''; });
                if (cells.every(function (c) { return c.textContent.trim() === ''; })) hideCol(th, cells);
                return;
            }
            if (kind === 'trend') {
                cells.forEach(function (c) {
                    c.style.backgroundColor = '';
                    var v = num(c);
                    c.classList.add(v === null ? 'rm-pt-z' : (v > 0 ? 'rm-pt-up' : 'rm-pt-dn'));
                });
                return;
            }
            if (kind === 'own' || kind === 'start') {
                cells.forEach(function (c, j) {
                    var b = c.querySelector('.pct-badge');
                    if (!b || vals[j] === null) return;
                    b.classList.add('rm-pt-orig');
                    if (b.dataset.bg) b.style.backgroundColor = b.dataset.bg;
                    var ch = b.querySelector('.rm-badge-change');
                    if (ch) {
                        var d = parseFloat(ch.textContent);
                        ch.classList.add(d > 0 ? 'rm-pt-up' : 'rm-pt-dn');
                        b.after(ch);
                    }
                });
                return;
            }
            var nonZero = vals.some(function (v) { return v !== null && Math.abs(v) >= 0.005; });
            if ((kind === 'stat' || kind === 'cval') && !nonZero) {
                th.classList.add('rm-pt-hide');
                cells.forEach(function (c) { c.classList.add('rm-pt-hide'); });
                return;
            }
            if (kind === 'stat') {
                cells.forEach(function (c, j) {
                    var b = c.querySelector('.rm-badge');
                    if (b) b.classList.add('rm-pt-plain');
                    if (vals[j] === null || Math.abs(vals[j]) < 0.005) {
                        c.classList.add('rm-pt-z');
                        if (b && b.textContent.trim() === '') b.textContent = '0';
                    }
                });
                return;
            }
            if (kind === 'val' || kind === 'cval') {
                cells.forEach(function (c, j) {
                    var b = c.querySelector('.value-badge, .rm-badge');
                    if (!b) return;
                    if (vals[j] !== null && Math.abs(vals[j]) < 0.005) { c.classList.add('rm-pt-z'); if (b.textContent.trim() === '-0.00') b.textContent = '0.00'; return; }
                    if (!b.dataset.bg) return;
                    b.classList.add('rm-pt-orig');
                    b.style.backgroundColor = b.dataset.bg;
                });
                return;
            }
            var max = 0;
            vals.forEach(function (v) { if (v !== null && Math.abs(v) > max) max = Math.abs(v); });
            cells.forEach(function (c, j) {
                var b = c.querySelector('.value-badge, .rm-badge');
                var v = vals[j];
                if (!b || v === null) return;
                if (Math.abs(v) < 0.005) { b.classList.add('rm-pt-plain'); c.classList.add('rm-pt-z'); return; }
                b.classList.add('rm-pt-tint', v > 0 ? 'rm-pt-p' : 'rm-pt-n');
                b.style.setProperty('--a', alpha(v, max));
            });
        });
        groups(table);
        lastCol(table);
        pin(table);
    }
    function all() {
        document.querySelectorAll('.rm-scores-table table.rm-pt').forEach(modern);
    }
    all();
    document.addEventListener('rm:live-updated', function () { setTimeout(all, 0); });
    window.addEventListener('resize', function () { document.querySelectorAll('.rm-scores-table table.rm-pt').forEach(pin); });
})();
