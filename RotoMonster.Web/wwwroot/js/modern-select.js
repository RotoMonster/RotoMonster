// Wraps a plain <select> in the library's bm-custom-select markup so it gets
// the modern trigger/arrow/panel look, WITHOUT changing the Razor markup.
//
// The native select stays in the DOM and keeps its name, value, asp-for
// binding and inline onchange - it is only hidden by CSS. Choosing an option
// writes the value back and dispatches a bubbling change event, so anything
// listening (including onchange="this.form.submit()") still fires.
//
// Opt in per select with class="rm-modern-select".
(function () {
    function closeAll(except) {
        var open = document.querySelectorAll('.bm-custom-select.open');
        Array.prototype.forEach.call(open, function (el) {
            if (el !== except) el.classList.remove('open');
        });
    }

    function enhance(select) {
        // Already wrapped, e.g. if this runs twice.
        if (select.parentNode && select.parentNode.classList
            && select.parentNode.classList.contains('bm-custom-select')) return;
        if (!select.options || select.options.length === 0) return;

        var wrap = document.createElement('div');
        wrap.className = 'bm-custom-select rm-modern-select-wrap';
        if (select.name) wrap.setAttribute('data-name', select.name);

        var trigger = document.createElement('div');
        trigger.className = 'bm-custom-select-trigger';

        var valueSpan = document.createElement('span');
        valueSpan.className = 'bm-custom-select-value';

        var arrow = document.createElement('span');
        arrow.className = 'bm-custom-select-arrow';

        trigger.appendChild(valueSpan);
        trigger.appendChild(arrow);

        var panel = document.createElement('div');
        panel.className = 'bm-custom-select-options';

        Array.prototype.forEach.call(select.options, function (opt) {
            var item = document.createElement('div');
            item.className = 'bm-custom-select-option';
            item.setAttribute('data-value', opt.value);
            item.textContent = opt.text;

            if (opt.selected) {
                item.classList.add('selected');
                valueSpan.textContent = opt.text;
            }

            item.addEventListener('click', function (e) {
                e.stopPropagation();

                var all = panel.querySelectorAll('.bm-custom-select-option');
                Array.prototype.forEach.call(all, function (o) {
                    o.classList.remove('selected');
                });
                item.classList.add('selected');
                valueSpan.textContent = opt.text;

                select.value = opt.value;
                select.dispatchEvent(new Event('change', { bubbles: true }));

                wrap.classList.remove('open');
            });

            panel.appendChild(item);
        });

        // A select with nothing marked selected still shows its first option.
        if (!valueSpan.textContent) valueSpan.textContent = select.options[0].text;

        select.parentNode.insertBefore(wrap, select);
        wrap.appendChild(trigger);
        wrap.appendChild(panel);
        wrap.appendChild(select);

        trigger.addEventListener('click', function (e) {
            e.stopPropagation();
            var wasOpen = wrap.classList.contains('open');
            closeAll(wrap);
            if (wasOpen) wrap.classList.remove('open');
            else wrap.classList.add('open');
        });
    }

    function init() {
        var selects = document.querySelectorAll('select.rm-modern-select');
        Array.prototype.forEach.call(selects, enhance);
        document.addEventListener('click', function () { closeAll(null); });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
