(function () {
    function enhance(root) {
        if (window.rmModernSelect && window.rmModernSelect.enhanceAll) window.rmModernSelect.enhanceAll(root);
    }

    function formDataFor(form, submitter) {
        var data;
        try {
            data = new FormData(form, submitter || undefined);
        } catch (err) {
            data = new FormData(form);
            if (submitter && submitter.name) data.append(submitter.name, submitter.value || '');
        }
        return data;
    }

    function nativeSubmit(form, submitter) {
        form._rmLiveOff = true;
        if (form.requestSubmit) form.requestSubmit(submitter || undefined);
        else form.submit();
    }

    function bind(form) {
        if (form._rmLiveBound) return;
        form._rmLiveBound = true;

        form.addEventListener('submit', function (e) {
            if (form._rmLiveOff || !window.fetch || !window.DOMParser || !form.id) return;
            e.preventDefault();

            var submitter = e.submitter || null;
            var action = submitter && submitter.hasAttribute('formaction') ? submitter.formAction : form.action;
            var data = formDataFor(form, submitter);

            if (form._rmLivePending) form._rmLivePending.abort();
            var pending = new AbortController();
            form._rmLivePending = pending;
            form.classList.add('rm-live-loading');

            fetch(action, { method: 'POST', body: data, credentials: 'same-origin', signal: pending.signal })
                .then(function (r) {
                    if (!r.ok) throw new Error('status ' + r.status);
                    return r.text().then(function (html) { return { html: html, url: r.url }; });
                })
                .then(function (res) {
                    var doc = new DOMParser().parseFromString(res.html, 'text/html');
                    var fresh = doc.getElementById(form.id);
                    if (!fresh) {
                        location.href = res.url;
                        return;
                    }
                    form.replaceWith(fresh);
                    if (res.url) history.replaceState(null, '', res.url);
                    enhance(fresh);
                    bind(fresh);
                    document.dispatchEvent(new CustomEvent('rm:live-updated', { detail: { root: fresh } }));
                })
                .catch(function (err) {
                    if (err.name === 'AbortError') return;
                    form.classList.remove('rm-live-loading');
                    nativeSubmit(form, submitter);
                });
        });
    }

    function swapIds(doc, ids) {
        ids.forEach(function (id) {
            var current = document.getElementById(id);
            var fresh = doc.getElementById(id);
            if (!current || !fresh) return;
            current.replaceWith(fresh);
            enhance(fresh);
            fresh.querySelectorAll('form[data-rm-live]').forEach(bind);
            document.dispatchEvent(new CustomEvent('rm:live-updated', { detail: { root: fresh } }));
        });
    }

    document.addEventListener('click', function (e) {
        var link = e.target.closest ? e.target.closest('a[data-rm-live]') : null;
        if (!link || !window.fetch || !window.DOMParser) return;
        if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey || e.button !== 0) return;
        e.preventDefault();
        if (link.classList.contains('rm-live-busy')) return;

        var ids = (link.getAttribute('data-rm-live') || '').split(/\s+/).filter(Boolean);
        var targets = ids.map(function (id) { return document.getElementById(id); }).filter(Boolean);
        if (!targets.length) {
            location.href = link.href;
            return;
        }

        link.classList.add('rm-live-busy');
        targets.forEach(function (t) { t.classList.add('rm-live-section', 'rm-live-loading'); });

        fetch(link.href, { credentials: 'same-origin' })
            .then(function (r) {
                if (!r.ok) throw new Error('status ' + r.status);
                return r.text();
            })
            .then(function (html) {
                var doc = new DOMParser().parseFromString(html, 'text/html');
                swapIds(doc, ids.concat(['rmMessages']));
                link.classList.remove('rm-live-busy');
            })
            .catch(function () {
                location.href = link.href;
            });
    });

    function init() {
        document.querySelectorAll('form[data-rm-live]').forEach(bind);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();
})();
