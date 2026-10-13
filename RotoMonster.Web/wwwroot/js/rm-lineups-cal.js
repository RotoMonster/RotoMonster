(function () {
    var btn = document.getElementById('luDateBtn');
    var cal = document.getElementById('luCal');
    if (!btn || !cal) return;
    var input = btn.querySelector('input');
    var form = btn.closest('form');

    function parse(s) {
        var p = (s || '').split('-');
        return p.length === 3 ? new Date(+p[0], +p[1] - 1, +p[2]) : new Date();
    }
    function iso(d) {
        return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
    }
    function same(a, b) {
        return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
    }

    var selected = parse(input.value);
    var today = parse(cal.getAttribute('data-today'));
    var view = new Date(selected.getFullYear(), selected.getMonth(), 1);
    var chevL = '<svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M10 3 5 8l5 5" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>';
    var chevR = '<svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="m6 3 5 5-5 5" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>';

    function render() {
        var start = new Date(view.getFullYear(), view.getMonth(), 1 - view.getDay());
        var html = '<div class="rm-lu-cal-head">'
            + '<button type="button" data-cal="-1" aria-label="Previous month">' + chevL + '</button>'
            + '<span>' + view.toLocaleDateString(undefined, { month: 'long', year: 'numeric' }) + '</span>'
            + '<button type="button" data-cal="1" aria-label="Next month">' + chevR + '</button>'
            + '</div><div class="rm-lu-cal-grid">';
        ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'].forEach(function (w) { html += '<span class="rm-lu-cal-wd">' + w + '</span>'; });
        for (var i = 0; i < 42; i++) {
            var d = new Date(start.getFullYear(), start.getMonth(), start.getDate() + i);
            var cls = 'rm-lu-cal-day';
            if (d.getMonth() !== view.getMonth()) cls += ' is-out';
            if (same(d, today)) cls += ' is-today';
            if (same(d, selected)) cls += ' is-sel';
            html += '<button type="button" class="' + cls + '" data-day="' + iso(d) + '">' + d.getDate() + '</button>';
        }
        html += '</div><div class="rm-lu-cal-foot"><button type="button" data-day="' + iso(today) + '">Today</button></div>';
        cal.innerHTML = html;
    }
    function open() {
        view = new Date(selected.getFullYear(), selected.getMonth(), 1);
        render();
        cal.hidden = false;
        var host = cal.offsetParent || document.body;
        var r = btn.getBoundingClientRect();
        var h = host.getBoundingClientRect();
        var left = Math.max(0, Math.min(r.left - h.left, h.width - cal.offsetWidth));
        cal.style.left = left + 'px';
        cal.style.top = (r.bottom - h.top + 6) + 'px';
        btn.setAttribute('aria-expanded', 'true');
    }
    function close() {
        cal.hidden = true;
        btn.setAttribute('aria-expanded', 'false');
    }

    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        if (cal.hidden) open(); else close();
    });
    btn.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); if (cal.hidden) open(); else close(); }
    });
    cal.addEventListener('click', function (e) {
        e.stopPropagation();
        var m = e.target.closest('[data-cal]');
        if (m) {
            view = new Date(view.getFullYear(), view.getMonth() + Number(m.getAttribute('data-cal')), 1);
            render();
            return;
        }
        var d = e.target.closest('[data-day]');
        if (d) {
            input.value = d.getAttribute('data-day');
            close();
            form.requestSubmit();
        }
    });
    document.addEventListener('click', close);
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(); });
})();
