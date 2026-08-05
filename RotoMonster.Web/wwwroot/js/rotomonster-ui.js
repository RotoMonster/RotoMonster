(function () {
    var activeTooltip = null;
    var activeArrow = null;

    function closeTooltip() {
        if (activeTooltip) {
            activeTooltip.classList.remove('bm-tooltip-visible');
            activeTooltip = null;
        }
        if (activeArrow) {
            activeArrow.remove();
            activeArrow = null;
        }
    }

    function showTooltip(trigger) {
        var id = trigger.getAttribute('data-bm-tooltip');
        var tooltip = document.getElementById(id);
        if (!tooltip) return;

        if (activeTooltip === tooltip) return;

        closeTooltip();

        var rect = trigger.getBoundingClientRect();
        var spaceAbove = rect.top;
        var spaceBelow = window.innerHeight - rect.bottom;
        var placeAbove = spaceAbove > 150 || spaceAbove > spaceBelow;

        if (tooltip.parentElement !== document.body) {
            document.body.appendChild(tooltip);
        }

        tooltip.classList.add('bm-tooltip-visible');

        var tw = tooltip.offsetWidth;
        var th = tooltip.offsetHeight;
        var left = rect.left + rect.width / 2 - tw / 2;
        left = Math.max(8, Math.min(left, window.innerWidth - tw - 8));

        var arrow = document.createElement('div');
        arrow.className = 'bm-tooltip-arrow';

        if (placeAbove) {
            tooltip.style.top = (rect.top - th - 10) + 'px';
            tooltip.style.left = left + 'px';
            arrow.classList.add('bm-tooltip-arrow--up');
            arrow.style.top = (rect.top - 10) + 'px';
            arrow.style.left = (rect.left + rect.width / 2 - 6) + 'px';
        } else {
            tooltip.style.top = (rect.bottom + 10) + 'px';
            tooltip.style.left = left + 'px';
            arrow.classList.add('bm-tooltip-arrow--down');
            arrow.style.top = (rect.bottom + 4) + 'px';
            arrow.style.left = (rect.left + rect.width / 2 - 6) + 'px';
        }

        document.body.appendChild(arrow);
        activeTooltip = tooltip;
        activeArrow = arrow;
    }

    document.addEventListener('click', function (e) {
        var trigger = e.target.closest('[data-bm-tooltip]');

        if (!trigger) {
            closeTooltip();
            return;
        }

        var id = trigger.getAttribute('data-bm-tooltip');
        var tooltip = document.getElementById(id);
        if (!tooltip) return;

        if (activeTooltip === tooltip) {
            closeTooltip();
            return;
        }

        showTooltip(trigger);
        e.stopPropagation();
    });

    // Hover-triggered tooltips (icon-only buttons) - opt-in via the
    // bm-tooltip-trigger--hover class. Click still works too as a touch
    // fallback, since hover doesn't exist on touch devices.
    document.addEventListener('mouseover', function (e) {
        var trigger = e.target.closest('.bm-tooltip-trigger--hover');
        if (!trigger) return;
        showTooltip(trigger);
    });

    document.addEventListener('mouseout', function (e) {
        var trigger = e.target.closest('.bm-tooltip-trigger--hover');
        if (!trigger) return;
        if (trigger.contains(e.relatedTarget)) return;
        closeTooltip();
    });
})();

$(document).on('show.bs.collapse', function(e) {
    var btn = $('[data-target="#' + e.target.id + '"]');
    btn.find('polyline').attr('points', '18 15 12 9 6 15');
    $('#' + e.target.id.replace('-content', '-toggle')).val('1');
});

$(document).on('hide.bs.collapse', function(e) {
    var btn = $('[data-target="#' + e.target.id + '"]');
    btn.find('polyline').attr('points', '6 9 12 15 18 9');
    $('#' + e.target.id.replace('-content', '-toggle')).val('0');
});

$(document).on('change', '.game-date-toggle-checkbox', function() {
    var row = $(this).closest('label').parent();
    if ($(this).is(':checked')) {
        row.addClass('game-date-row--selected');
    } else {
        row.removeClass('game-date-row--selected');
    }
});

// Popup Calendar - toggle open/close
document.addEventListener('click', function(e) {
    var trigger = e.target.closest('[data-popup-cal]');

    if (trigger) {
        var panelId = trigger.getAttribute('data-popup-cal');
        var panel = document.getElementById(panelId);
        if (!panel) return;
        var isOpen = panel.classList.contains('popup-cal-open');
        document.querySelectorAll('.popup-cal-panel.popup-cal-open')
            .forEach(function(p) { p.classList.remove('popup-cal-open'); });
        if (!isOpen) {
            if (panel.parentElement !== document.body) {
                document.body.appendChild(panel);
            }
            var triggerRect = trigger.getBoundingClientRect();
            panel.style.position = 'absolute';
            panel.style.top = (triggerRect.bottom + window.scrollY + 4) + 'px';
            panel.style.left = (triggerRect.left + window.scrollX) + 'px';
            panel.classList.add('popup-cal-open');
        }
        e.stopPropagation();
        return;
    }

    // Click outside closes all
    if (!e.target.closest('.popup-cal-panel')) {
        document.querySelectorAll('.popup-cal-panel.popup-cal-open')
            .forEach(function(p) { p.classList.remove('popup-cal-open'); });
    }
});

// Popup Calendar - day selection
document.addEventListener('click', function(e) {
    var day = e.target.closest('[data-popup-cal-date]');
    if (!day) return;

    // Skip - handled by DateNavControl handler
    if (day.getAttribute('data-date-nav-cal')) return;

    e.stopPropagation();

    if (day.classList.contains('popup-cal-day--disabled')) return;

    var dateVal = day.getAttribute('data-popup-cal-date');
    var panel = day.closest('.popup-cal-panel');
    if (!panel) return;
    var wrapperId = panel.id.replace('-panel', '');
    var wrapper = document.getElementById(wrapperId);
    if (wrapper && wrapper.getAttribute('data-mode') === 'range') return;

    // Update hidden field
    var hidden = document.getElementById(wrapperId + '-selected');
    if (hidden) hidden.value = dateVal;

    // Update trigger button text and icon
    var trigger = document.querySelector('[data-popup-cal="' + panel.id + '"]');
    if (trigger) {
        var date = new Date(dateVal + 'T00:00:00');
        var formatted = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

        var svgText = trigger.querySelector('svg text');
        if (svgText) svgText.textContent = date.getDate();

        var label = trigger.querySelector('.popup-cal-trigger-label');
        if (label) label.textContent = formatted;
    }

// Update selected day styling
    var panel = document.getElementById(navId + '-cal-panel');
    if (panel) {
        panel.querySelectorAll('.popup-cal-day--selected').forEach(function(d) {
            d.classList.remove('popup-cal-day--selected');
        });
        day.classList.add('popup-cal-day--selected');
        panel.classList.remove('popup-cal-open');
    }

    // Mark that the calendar was used to change the date
    var calendarChangedFlag = wrapper ? wrapper.querySelector('input[name="' + navId + '-calendar-changed"]') : null;
    if (calendarChangedFlag) calendarChangedFlag.value = '1';

    // Auto-submit the form so the games list refreshes without needing a manual Refresh click
    if (wrapper) {
        var form = wrapper.closest('form');
        if (form) {
            // requestSubmit fires the form's onsubmit, which is what ASP.NET
            // AJAX hooks. submit() skips it and forces a full page refresh.
            if (form.requestSubmit) form.requestSubmit();
            else form.submit();
        }
    }
});

// Popup Calendar - month navigation
document.addEventListener('click', function(e) {
    var isPrev = e.target.closest('[data-popup-cal-prev]');
    var isNext = e.target.closest('[data-popup-cal-next]');
    if (!isPrev && !isNext) return;

    e.stopPropagation();

    var btn = isPrev || isNext;
    var wrapperId = btn.getAttribute(isPrev ? 'data-popup-cal-prev' : 'data-popup-cal-next');
    var panel = document.getElementById(wrapperId + '-cal-panel') || document.getElementById(wrapperId + '-panel');
    if (!panel) return;

    var monthStr = panel.getAttribute('data-month');
    var parts = monthStr.split('-');
    var year = parseInt(parts[0]);
    var month = parseInt(parts[1]) - 1;
    var current = new Date(year, month, 1);

    var newMonth = isPrev
        ? new Date(current.getFullYear(), current.getMonth() - 1, 1)
        : new Date(current.getFullYear(), current.getMonth() + 1, 1);

    var newMonthStr = newMonth.getFullYear() + '-' + String(newMonth.getMonth() + 1).padStart(2, '0');
    panel.setAttribute('data-month', newMonthStr);

    var monthHidden = document.getElementById(wrapperId + '-month');
    if (monthHidden) monthHidden.value = newMonthStr;

    var monthLabel = panel.querySelector('.popup-cal-month-label');
    if (monthLabel) {
        var monthNames = ['January','February','March','April','May','June','July','August','September','October','November','December'];
        monthLabel.textContent = monthNames[newMonth.getMonth()] + ' ' + newMonth.getFullYear();
    }

    var isDateNav = panel.hasAttribute('data-date-nav-trigger');
    var dateNavId = panel.getAttribute('data-date-nav-trigger');
    var dateNavTarget = panel.getAttribute('data-date-nav-target');
    
    var selectedHidden = isDateNav
        ? document.getElementById(dateNavTarget)
        : document.getElementById(wrapperId + '-selected');
    var selectedDateStr = selectedHidden ? selectedHidden.value : '';
    
    var grid = panel.querySelector('.popup-cal-grid');
    if (!grid) return;
    grid.innerHTML = '';

    var firstOfMonth = new Date(newMonth.getFullYear(), newMonth.getMonth(), 1);
    var daysInMonth = new Date(newMonth.getFullYear(), newMonth.getMonth() + 1, 0).getDate();
    var startDayOfWeek = firstOfMonth.getDay();
    var today = new Date();
    today.setHours(0, 0, 0, 0);

    for (var i = 0; i < startDayOfWeek; i++) {
        var empty = document.createElement('span');
        empty.className = 'popup-cal-day popup-cal-day--empty';
        grid.appendChild(empty);
    }

    for (var day = 1; day <= daysInMonth; day++) {
        var date = new Date(newMonth.getFullYear(), newMonth.getMonth(), day);
        var dateStr = date.getFullYear() + '-' + String(date.getMonth() + 1).padStart(2, '0') + '-' + String(date.getDate()).padStart(2, '0');

        var classes = 'popup-cal-day';
        if (date.getTime() === today.getTime()) classes += ' popup-cal-day--today';
        if (selectedDateStr === dateStr) classes += ' popup-cal-day--selected';

        var dayBtn = document.createElement('button');
        dayBtn.type = 'button';
        dayBtn.className = classes;
        dayBtn.setAttribute('data-popup-cal-date', dateStr);
        dayBtn.setAttribute('data-popup-cal-target', wrapperId + '-selected');
        if (isDateNav) {
            dayBtn.setAttribute('data-date-nav-cal', dateNavId);
        }
        dayBtn.textContent = day;
        grid.appendChild(dayBtn);
    }
});

// Popup Calendar - Range selection
(function() {
    var rangeState = {};

    function getRangeState(wrapperId) {
        if (!rangeState[wrapperId]) {
            rangeState[wrapperId] = { awaiting: 'start', start: null };
        }
        return rangeState[wrapperId];
    }

    function clearRangePreview(panel) {
        panel.querySelectorAll('.popup-cal-day--range-preview').forEach(function(d) {
            d.classList.remove('popup-cal-day--range-preview');
        });
    }

    function updateRangeHighlight(panel, startStr, endStr) {
        panel.querySelectorAll('.popup-cal-day').forEach(function(d) {
            d.classList.remove('popup-cal-day--range-start', 'popup-cal-day--range-end', 'popup-cal-day--in-range', 'popup-cal-day--range-preview');
            var dateStr = d.getAttribute('data-popup-cal-date');
            if (!dateStr) return;
            if (dateStr === startStr) d.classList.add('popup-cal-day--range-start');
            if (dateStr === endStr) d.classList.add('popup-cal-day--range-end');
            if (startStr && endStr && dateStr > startStr && dateStr < endStr)
                d.classList.add('popup-cal-day--in-range');
        });
    }

    function updateTriggerLabel(wrapperId, startStr, endStr) {
        var trigger = document.querySelector('[data-popup-cal="' + wrapperId + '-panel"]');
        if (!trigger) return;
        var label = trigger.querySelector('.popup-cal-trigger-label');
        if (!label) return;

        if (startStr && endStr) {
            var s = new Date(startStr + 'T00:00:00');
            var e = new Date(endStr + 'T00:00:00');
            label.textContent = s.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) + ' – ' + e.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
        } else if (startStr) {
            var s = new Date(startStr + 'T00:00:00');
            label.textContent = s.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) + ' – Select End';
        }
    }

    document.addEventListener('click', function(e) {
        var day = e.target.closest('[data-popup-cal-date]');
        if (!day) return;

        var panel = day.closest('.popup-cal-panel');
        if (!panel) return;

        var wrapperId = panel.id.replace('-panel', '');
        var wrapper = document.getElementById(wrapperId);
        if (!wrapper || wrapper.getAttribute('data-mode') !== 'range') return;

        e.stopPropagation();

        if (day.classList.contains('popup-cal-day--disabled')) return;

        var dateVal = day.getAttribute('data-popup-cal-date');
        var state = getRangeState(wrapperId);

        if (state.awaiting === 'start') {
            state.start = dateVal;
            state.awaiting = 'end';

            document.getElementById(wrapperId + '-start').value = dateVal;
            document.getElementById(wrapperId + '-end').value = '';

            updateRangeHighlight(panel, dateVal, null);
        } else {
            var startVal = state.start;
            var endVal = dateVal;

            if (endVal < startVal) {
                var temp = startVal;
                startVal = endVal;
                endVal = temp;
            }

            document.getElementById(wrapperId + '-start').value = startVal;
            document.getElementById(wrapperId + '-end').value = endVal;

            updateRangeHighlight(panel, startVal, endVal);
            updateTriggerLabel(wrapperId, startVal, endVal);

            state.start = null;
            state.awaiting = 'start';

            panel.classList.remove('popup-cal-open');
        }
    });

    document.addEventListener('mouseover', function(e) {
        var day = e.target.closest('[data-popup-cal-date]');
        if (!day) return;

        var panel = day.closest('.popup-cal-panel');
        if (!panel) return;

        var wrapperId = panel.id.replace('-panel', '');
        var wrapper = document.getElementById(wrapperId);
        if (!wrapper || wrapper.getAttribute('data-mode') !== 'range') return;

        var state = getRangeState(wrapperId);
        if (state.awaiting !== 'end' || !state.start) return;

        var hoverDate = day.getAttribute('data-popup-cal-date');
        clearRangePreview(panel);

        panel.querySelectorAll('.popup-cal-day').forEach(function(d) {
            var dateStr = d.getAttribute('data-popup-cal-date');
            if (!dateStr) return;
            if (hoverDate > state.start && dateStr > state.start && dateStr < hoverDate)
                d.classList.add('popup-cal-day--range-preview');
            if (hoverDate < state.start && dateStr < state.start && dateStr > hoverDate)
                d.classList.add('popup-cal-day--range-preview');
        });
    });
})();

// Rich Text Editor
document.querySelectorAll('.rte-btn').forEach(function(btn) {
    btn.addEventListener('click', function(e) {
        e.preventDefault();
        var cmd = btn.getAttribute('data-rte-cmd');
        var prompt = btn.getAttribute('data-rte-prompt');

        if (cmd === 'createLink') {
            var url = window.prompt(prompt || 'Enter URL:');
            if (url) document.execCommand(cmd, false, url);
        } else {
            document.execCommand(cmd, false, null);
        }

        updateRteActiveStates(btn.closest('.rte-wrapper'));
        syncRteValue(btn.closest('.rte-wrapper'));
    });
});

document.querySelectorAll('.rte-body').forEach(function(editor) {
    editor.addEventListener('input', function() {
        syncRteValue(editor.closest('.rte-wrapper'));
    });
    editor.addEventListener('keyup', function() {
        updateRteActiveStates(editor.closest('.rte-wrapper'));
    });
    editor.addEventListener('mouseup', function() {
        updateRteActiveStates(editor.closest('.rte-wrapper'));
    });
});

function syncRteValue(wrapper) {
    if (!wrapper) return;
    var editor = wrapper.querySelector('.rte-body');
    var hidden = wrapper.querySelector('input[type="hidden"]');
    if (editor && hidden) {
        var utf8Bytes = new TextEncoder().encode(editor.innerHTML);
        var binary = '';
        utf8Bytes.forEach(function (b) { binary += String.fromCharCode(b); });
        hidden.value = btoa(binary);
    }
}

function updateRteActiveStates(wrapper) {
    if (!wrapper) return;
    wrapper.querySelectorAll('.rte-btn[data-rte-cmd]').forEach(function(btn) {
        var cmd = btn.getAttribute('data-rte-cmd');
        try {
            if (document.queryCommandState(cmd)) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        } catch(e) {}
    });
}

// Emoji picker toggle
document.addEventListener('click', function (e) {
    const emojiBtn = e.target.closest('[data-rte-emoji-panel]');
    if (emojiBtn) {
        e.stopPropagation();
        const panelId = emojiBtn.getAttribute('data-rte-emoji-panel');
        const panel = document.getElementById(panelId);
        if (panel) {
            const isVisible = panel.style.display !== 'none';
            document.querySelectorAll('.rte-emoji-panel').forEach(p => p.style.display = 'none');
            panel.style.display = isVisible ? 'none' : 'block';
            if (!isVisible) {
                const btnRect = emojiBtn.getBoundingClientRect();
                panel.style.top = (btnRect.bottom + 4) + 'px';
                panel.style.left = btnRect.left + 'px';
            }
        }
        return;
    }

    const emojiItem = e.target.closest('[data-emoji]');
    if (emojiItem) {
        const emoji = emojiItem.getAttribute('data-emoji');
        const editorId = emojiItem.getAttribute('data-rte-editor');
        const editor = document.getElementById(editorId);
        if (editor) {
            editor.focus();
            document.execCommand('insertText', false, emoji);
        }
        const panel = emojiItem.closest('.rte-emoji-panel');
        if (panel) panel.style.display = 'none';
        return;
    }

    document.querySelectorAll('.rte-emoji-panel').forEach(p => p.style.display = 'none');
});

// DateNavControl calendar day selection
document.addEventListener('click', function(e) {
    var day = e.target.closest('[data-popup-cal-date][data-date-nav-cal]');
    if (!day) return;

    e.stopPropagation();

    var dateVal = day.getAttribute('data-popup-cal-date');
    var navId = day.getAttribute('data-date-nav-cal');

    // Update hidden date field - find within same wrapper
    var wrapper = document.getElementById(navId);
    var hidden = wrapper ? wrapper.querySelector('input[name="' + navId + '-date"]') : null;
    if (hidden) hidden.value = dateVal;

    // Update trigger label
    var trigger = document.querySelector('[data-date-nav-cal="' + navId + '"][data-popup-cal]');
    if (trigger) {
        var date = new Date(dateVal + 'T00:00:00');
        var formatted = date.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });
        var label = trigger.querySelector('.popup-cal-trigger-label');
        if (label) label.textContent = formatted;
        var svgText = trigger.querySelector('svg text');
        if (svgText) svgText.textContent = date.getDate();
    }

    // Update selected day styling
    var panel = document.getElementById(navId + '-cal-panel');
    if (panel) {
        panel.querySelectorAll('.popup-cal-day--selected').forEach(function(d) {
            d.classList.remove('popup-cal-day--selected');
        });
        day.classList.add('popup-cal-day--selected');
        panel.classList.remove('popup-cal-open');
    }

    // Mark that the calendar was used to change the date
    var calendarChangedFlag = wrapper ? wrapper.querySelector('input[name="' + navId + '-calendar-changed"]') : null;
    if (calendarChangedFlag) calendarChangedFlag.value = '1';

    // Auto-submit the form so the games list refreshes without needing a manual Refresh click
    if (wrapper) {
        var form = wrapper.closest('form');
        if (form) form.submit();
    }
});

// Tour engine
(function() {
    function startTour(tourEl) {
        var steps = JSON.parse(tourEl.getAttribute('data-tour-steps'));
        var current = 0;

        var overlay = document.createElement('div');
        overlay.className = 'bm-tour-tooltip';
        document.body.appendChild(overlay);

        function clearPulse() {
            document.querySelectorAll('.bm-tour-pulse').forEach(function(p) { p.remove(); });
        }

        function render() {
            clearPulse();
            var step = steps[current];
            var target = document.getElementById(step.targetId ? step.targetId : step.TargetId);
            if (!target) return;

            target.scrollIntoView({ behavior: 'smooth', block: 'center' });

            setTimeout(function() {
                positionAndShow(step, target);
            }, 350);
        }

        function positionAndShow(step, target) {
            var text = step.text || step.Text;
            var position = step.position || step.Position || 'Bottom';

            target.style.position = 'relative';
            var pulse = document.createElement('span');
            pulse.className = 'bm-tour-pulse';
            target.appendChild(pulse);

            var rect = target.getBoundingClientRect();
            var top, left;
            if (position === 'Top' || position === 0) {
                top = rect.top + window.scrollY - 12;
                left = rect.left + window.scrollX;
                overlay.style.transform = 'translateY(-100%)';
            } else {
                top = rect.bottom + window.scrollY + 12;
                left = rect.left + window.scrollX;
                overlay.style.transform = 'none';
            }

            overlay.style.top = top + 'px';
            overlay.style.left = left + 'px';
            overlay.innerHTML =
                '<p class="bm-tour-step-count">Step ' + (current + 1) + ' of ' + steps.length + '</p>' +
                '<p class="bm-tour-text">' + text + '</p>' +
                '<div class="bm-tour-controls">' +
                    '<span class="bm-tour-skip">Skip tour</span>' +
                    '<div class="bm-tour-buttons">' +
                        '<button class="bm-tour-back" ' + (current === 0 ? 'style="visibility:hidden;"' : '') + '>Back</button>' +
                        '<button class="bm-tour-next">' + (current === steps.length - 1 ? 'Done' : 'Next') + '</button>' +
                    '</div>' +
                '</div>';

            overlay.querySelector('.bm-tour-next').addEventListener('click', function() {
                if (current < steps.length - 1) {
                    current++;
                    render();
                } else {
                    endTour();
                }
            });
            overlay.querySelector('.bm-tour-back').addEventListener('click', function() {
                if (current > 0) {
                    current--;
                    render();
                }
            });
            overlay.querySelector('.bm-tour-skip').addEventListener('click', endTour);
        }

        function endTour() {
            clearPulse();
            overlay.remove();
        }

        render();
    }

    document.querySelectorAll('.bm-tour').forEach(function(tourEl) {
        if (tourEl.hasAttribute('data-manual-trigger')) return;
        startTour(tourEl);
    });

    window.RotoMonsterStartTour = function(tourId) {
        var tourEl = document.getElementById(tourId);
        if (tourEl) startTour(tourEl);
    };
})();

// Page guide (purpose + how-to modal, companion to Tour)
(function() {
    document.querySelectorAll('.bm-page-guide-trigger').forEach(function(trigger) {
        trigger.addEventListener('click', function() {
            var modal = document.getElementById(trigger.getAttribute('data-guide-target'));
            if (modal) modal.style.display = 'flex';
        });
    });

    document.querySelectorAll('.bm-page-guide-modal').forEach(function(modal) {
        function closeModal() { modal.style.display = 'none'; }

        var closeX = modal.querySelector('.bm-page-guide-close');
        if (closeX) closeX.addEventListener('click', closeModal);

        var closeBtn = modal.querySelector('.bm-page-guide-close-btn');
        if (closeBtn) closeBtn.addEventListener('click', closeModal);

        modal.addEventListener('click', function(e) {
            if (e.target === modal) closeModal();
        });

        modal.querySelectorAll('.bm-page-guide-section-header').forEach(function(header) {
            header.addEventListener('click', function() {
                header.parentElement.classList.toggle('bm-page-guide-section--open');
            });
        });

        var tourBtn = modal.querySelector('.bm-page-guide-tour-btn');
        if (tourBtn) {
            tourBtn.addEventListener('click', function() {
                closeModal();
                if (window.RotoMonsterStartTour) {
                    window.RotoMonsterStartTour(tourBtn.getAttribute('data-start-tour'));
                }
            });
        }
    });
})();

function EditNews(btn) {
    var newsId = btn.getAttribute('data-newsid');
    __doPostBack('editnews_' + newsId, 'edit', btn.closest('form'));
}

function DeleteNews(btn) {
    var newsId = btn.getAttribute('data-newsid');
    __doPostBack('deletenews_' + newsId, 'delete', btn.closest('form'));
}

function TriggerPostBack(btn, prefix, dataAttr, argument) {
    var id = btn.getAttribute(dataAttr);
    __doPostBack(prefix + id, argument || '', btn.closest('form'));
}

function DeleteChat(btn) {
    var messageId = btn.getAttribute('data-messageid');
    __doPostBack('deletechat_' + messageId, 'delete', btn.closest('form'));
}

if (typeof window.__doPostBack !== 'function') {
    window.__doPostBack = function (eventTarget, eventArgument, form) {
        form = form || document.forms[0];
        if (!form) return;

        var et = form.querySelector('input[name="__EVENTTARGET"]');
        if (!et) {
            et = document.createElement('input');
            et.type = 'hidden';
            et.name = '__EVENTTARGET';
            form.appendChild(et);
        }
        et.value = eventTarget;

        var ea = form.querySelector('input[name="__EVENTARGUMENT"]');
        if (!ea) {
            ea = document.createElement('input');
            ea.type = 'hidden';
            ea.name = '__EVENTARGUMENT';
            form.appendChild(ea);
        }
        ea.value = eventArgument || '';

        form.submit();
    };
}

// Poll player picker - client-side search/filter against the embedded AvailablePlayers list
document.addEventListener('input', function (e) {
    if (!e.target.matches('.poll-player-picker input[id$="-search"]')) return;
    var wrapper = e.target.closest('.poll-player-picker');
    if (!wrapper) return;
    var baseId = wrapper.id;
    var dataEl = document.getElementById(baseId + '-data');
    var resultsEl = document.getElementById(baseId + '-results');
    if (!dataEl || !resultsEl) return;

    var query = e.target.value.trim().toLowerCase();
    resultsEl.innerHTML = '';
    if (!query) { resultsEl.style.display = 'none'; return; }

    var players;
    try { players = JSON.parse(dataEl.textContent); } catch (err) { return; }

    // Match on player name first, then on any alias (aliases are searchable but never shown).
    var nameMatches = [];
    var aliasMatches = [];
    for (var pi = 0; pi < players.length; pi++) {
        var candidate = players[pi];
        if (candidate.name && candidate.name.toLowerCase().indexOf(query) !== -1) {
            nameMatches.push(candidate);
            continue;
        }
        var aliases = candidate.aliases || [];
        for (var ai = 0; ai < aliases.length; ai++) {
            if (aliases[ai] && aliases[ai].toLowerCase().indexOf(query) !== -1) {
                aliasMatches.push(candidate);
                break;
            }
        }
    }
    var matches = nameMatches.concat(aliasMatches).slice(0, 8);

    if (matches.length === 0) { resultsEl.style.display = 'none'; return; }

    matches.forEach(function (p) {
        var li = document.createElement('li');
        li.className = 'listItem';
        li.textContent = p.name + (p.team ? ' (' + p.team + (p.pos ? ' ' + p.pos : '') + ')' : '');
        li.addEventListener('click', function () {
            var selectedInput = document.getElementById(baseId + '-selected');
            var searchInput = document.getElementById(baseId + '-search');
            if (selectedInput) selectedInput.value = p.id;
            if (searchInput) searchInput.value = p.name;
            resultsEl.innerHTML = '';
            resultsEl.style.display = 'none';
        });
        resultsEl.appendChild(li);
    });

    resultsEl.style.display = 'block';
});

document.addEventListener('click', function (e) {
    if (e.target.closest('.poll-player-picker-search-row')) return;
    document.querySelectorAll('.poll-player-picker-results').forEach(function (el) {
        el.style.display = 'none';
    });
});

// Page title row player search - same embedded-JSON pattern as the poll picker
document.addEventListener('input', function (e) {
    if (!e.target.matches('.player-search input[id$="-search"]')) return;
    var wrapper = e.target.closest('.player-search');
    if (!wrapper) return;
    var baseId = wrapper.id;
    var dataEl = document.getElementById(baseId + '-data');
    var resultsEl = document.getElementById(baseId + '-results');
    if (!dataEl || !resultsEl) return;

    var query = e.target.value.trim().toLowerCase();
    resultsEl.innerHTML = '';
    if (!query) { resultsEl.style.display = 'none'; return; }

    var players;
    try { players = JSON.parse(dataEl.textContent); } catch (err) { return; }

    var max = parseInt(wrapper.getAttribute('data-maxresults'), 10) || 8;
    var urlFormat = wrapper.getAttribute('data-urlformat');

    // Match on player name first, then on any alias (aliases are searchable but never shown).
    var nameMatches = [];
    var aliasMatches = [];
    for (var pi = 0; pi < players.length; pi++) {
        var candidate = players[pi];
        if (candidate.name && candidate.name.toLowerCase().indexOf(query) !== -1) {
            nameMatches.push(candidate);
            continue;
        }
        var aliases = candidate.aliases || [];
        for (var ai = 0; ai < aliases.length; ai++) {
            if (aliases[ai] && aliases[ai].toLowerCase().indexOf(query) !== -1) {
                aliasMatches.push(candidate);
                break;
            }
        }
    }
    var matches = nameMatches.concat(aliasMatches).slice(0, max);

    if (matches.length === 0) { resultsEl.style.display = 'none'; return; }

    matches.forEach(function (p) {
        var li = document.createElement('li');
        li.className = 'listItem';
        li.textContent = p.name + (p.team ? ' (' + p.team + (p.pos ? ' ' + p.pos : '') + ')' : '');
        li.addEventListener('click', function () {
            if (urlFormat) {
                window.location.href = urlFormat.replace('{id}', p.id);
                return;
            }
            var selectedInput = document.getElementById(baseId + '-selected');
            var searchInput = document.getElementById(baseId + '-search');
            if (selectedInput) selectedInput.value = p.id;
            if (searchInput) searchInput.value = p.name;
            resultsEl.innerHTML = '';
            resultsEl.style.display = 'none';
            if (typeof __doPostBack === 'function') __doPostBack(baseId + '-selected', '');
        });
        resultsEl.appendChild(li);
    });

    resultsEl.style.display = 'block';
});

document.addEventListener('click', function (e) {
    if (e.target.closest('.player-search')) return;
    document.querySelectorAll('.player-search-results').forEach(function (el) {
        el.style.display = 'none';
    });
});
/* ============================================
   STAT CHARTS (Google Charts)
   Draws any .bm-chart container from its sibling
   <script type="application/json"> spec. Themes
   from CSS variables so light/dark just work.
   ============================================ */
(function () {
    function themeColors() {
        var cs = getComputedStyle(document.documentElement);
        function v(name, fallback) {
            var val = cs.getPropertyValue(name).trim();
            return val || fallback;
        }

        // Series colors come from CSS so the sport files and light/dark can
        // retheme them. Looked up by index, so defining more in CSS just works.
        // The fallbacks only kick in past whatever CSS defines.
        var seriesFallbacks = ['#e66000', '#ff8c42', '#378add', '#1d9e75', '#d4537e', '#ba7517'];
        function seriesColor(i) {
            var fromCss = v('--chart-series-' + (i + 1), '');
            return fromCss || seriesFallbacks[i % seriesFallbacks.length];
        }

        return {
            text: v('--color-text-primary', '#1e293b'),
            grid: v('--color-border', '#e2e8f0'),
            brand: v('--brand-primary', '#e66000'),
            accent: v('--brand-accent', '#ff8c42'),
            seriesColor: seriesColor
        };
    }

    // A colorCSS can be a var() reference. Google Charts only takes real
    // values, so resolve it here. Returns null when the variable doesn't
    // exist, so the caller falls back to the palette instead of drawing blank.
    function resolveColor(c) {
        if (!c) return null;
        var m = /^var\(\s*(--[\w-]+)\s*\)$/.exec(c);
        if (!m) return c;
        var val = getComputedStyle(document.documentElement)
            .getPropertyValue(m[1]).trim();
        return val || null;
    }

    function seriesColors(theme, series) {
        var out = [];
        for (var i = 0; i < series.length; i++) {
            out.push(resolveColor(series[i] && series[i].colorCSS) || theme.seriesColor(i));
        }
        return out;
    }

    function buildDataTable(spec) {
        var data = new google.visualization.DataTable();
        data.addColumn('string', spec.xAxisLabel || '');
        spec.series.forEach(function (s) { data.addColumn('number', s.name || ''); });

        // X categories in first-seen order across all series.
        var xOrder = [], seen = {};
        spec.series.forEach(function (s) {
            (s.points || []).forEach(function (p) {
                if (!seen[p.x]) { seen[p.x] = true; xOrder.push(p.x); }
            });
        });
        var maps = spec.series.map(function (s) {
            var m = {};
            (s.points || []).forEach(function (p) { m[p.x] = p.y; });
            return m;
        });
        xOrder.forEach(function (x) {
            var row = [x];
            maps.forEach(function (m) { row.push(m.hasOwnProperty(x) ? m[x] : null); });
            data.addRow(row);
        });
        return data;
    }

    function drawChart(el, spec) {
        var theme = themeColors();
        var data = buildDataTable(spec);

        var options = {
            title: spec.title || '',
            titleTextStyle: { color: theme.text },
            backgroundColor: 'transparent',
            colors: seriesColors(theme, spec.series),
            legend: spec.series.length > 1 ? { position: 'top', textStyle: { color: theme.text } } : 'none',
            chartArea: { width: '85%', height: '70%' },
            hAxis: {
                title: spec.xAxisLabel || '',
                textStyle: { color: theme.text },
                titleTextStyle: { color: theme.text, italic: false },
                gridlines: { color: theme.grid },
                baselineColor: theme.grid
            },
            vAxis: {
                title: spec.yAxisLabel || '',
                textStyle: { color: theme.text },
                titleTextStyle: { color: theme.text, italic: false },
                gridlines: { color: theme.grid },
                baselineColor: theme.grid
            }
        };
        
        // Optional hard Y bounds. viewWindow clamps; minValue/maxValue only
        // extend, so viewWindow is what actually honors Ken's low/high.
        var hasMin = spec.yAxisMin !== undefined && spec.yAxisMin !== null;
        var hasMax = spec.yAxisMax !== undefined && spec.yAxisMax !== null;
        if (hasMin || hasMax) {
            options.vAxis.viewWindow = {};
            if (hasMin) options.vAxis.viewWindow.min = spec.yAxisMin;
            if (hasMax) options.vAxis.viewWindow.max = spec.yAxisMax;
        }

        var chart;
        if (spec.type === 'bar') {
            chart = new google.visualization.ColumnChart(el);
        } else {
            options.curveType = 'function';
            options.pointSize = 4;
            chart = new google.visualization.LineChart(el);
        }
        chart.draw(data, options);
    }

    function drawAllCharts() {
        var containers = document.querySelectorAll('.bm-chart');
        if (!containers.length) return;
        containers.forEach(function (el) {
            var spec = el._bmSpec;
            if (!spec) {
                var dataEl = document.getElementById(el.id + '-chartdata');
                if (!dataEl) return;
                try { spec = JSON.parse(dataEl.textContent); } catch (e) { return; }
                el._bmSpec = spec;
            }
            drawChart(el, spec);
        });
    }

    function loadThenDraw() {
        if (!document.querySelector('.bm-chart')) return;
        if (window.google && window.google.charts) {
            google.charts.load('current', { packages: ['corechart'] });
            google.charts.setOnLoadCallback(drawAllCharts);
        } else {
            var s = document.createElement('script');
            s.src = 'https://www.gstatic.com/charts/loader.js';
            s.onload = function () {
                google.charts.load('current', { packages: ['corechart'] });
                google.charts.setOnLoadCallback(drawAllCharts);
            };
            document.head.appendChild(s);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', loadThenDraw);
    } else {
        loadThenDraw();
    }

    var resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(drawAllCharts, 200);
    });
})();

// Favorites toolbar - drag a pill's handle to reorder. Reorders the DOM live
// during drag, then posts the new order back on drop (skips if unchanged).
(function () {
    var dragEl = null;

    document.addEventListener('dragstart', function (e) {
        var handle = e.target.closest('.favorites-toolbar-handle[draggable="true"]');
        if (!handle) return;
        dragEl = handle.closest('.favorites-toolbar-pill');
        if (!dragEl) return;
        dragEl.classList.add('favorites-toolbar-pill--dragging');
        e.dataTransfer.effectAllowed = 'move';
        // Firefox needs setData to start a drag; use the pill as the drag image.
        try { e.dataTransfer.setData('text/plain', dragEl.getAttribute('data-pageid') || ''); } catch (err) {}
        try { e.dataTransfer.setDragImage(dragEl, 10, 10); } catch (err) {}
    });

    document.addEventListener('dragover', function (e) {
        if (!dragEl) return;
        var bar = e.target.closest('.favorites-toolbar');
        if (!bar || dragEl.closest('.favorites-toolbar') !== bar) return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';

        var afterEl = getDragAfterElement(bar, e.clientX);
        if (afterEl == null) {
            // Keep the add/remove toggle button pinned at the end.
            var toggle = bar.querySelector('.favorites-toolbar-current-btn');
            if (toggle) bar.insertBefore(dragEl, toggle);
            else bar.appendChild(dragEl);
        } else {
            bar.insertBefore(dragEl, afterEl);
        }
    });

    document.addEventListener('dragend', function () {
        if (!dragEl) return;
        dragEl.classList.remove('favorites-toolbar-pill--dragging');
        var bar = dragEl.closest('.favorites-toolbar');
        dragEl = null;
        if (bar) commitOrder(bar);
    });

    function getDragAfterElement(bar, x) {
        var pills = Array.prototype.slice.call(
            bar.querySelectorAll('.favorites-toolbar-pill:not(.favorites-toolbar-pill--dragging)')
        );
        var closest = null;
        var closestOffset = Number.NEGATIVE_INFINITY;
        pills.forEach(function (pill) {
            var box = pill.getBoundingClientRect();
            var offset = x - box.left - box.width / 2;
            if (offset < 0 && offset > closestOffset) {
                closestOffset = offset;
                closest = pill;
            }
        });
        return closest;
    }

    function commitOrder(bar) {
        var id = bar.getAttribute('data-favorites-id');
        if (!id) return;

        var ids = Array.prototype.slice.call(bar.querySelectorAll('.favorites-toolbar-pill'))
            .map(function (p) { return p.getAttribute('data-pageid'); })
            .filter(function (v) { return v; });
        var next = ids.join(',');

        var hidden = document.getElementById(id + '_order');
        var prev = hidden ? hidden.value : '';
        if (hidden) hidden.value = next;

        if (next === prev) return; // dropped in the same place - no postback
        if (typeof __doPostBack === 'function') {
            __doPostBack(id + '_reorder', next, bar.closest('form'));
        }
    }
})();

document.addEventListener('change', function (e) {
    var check = e.target.closest('.tweet-card-player-check');
    if (!check) return;

    var list = check.closest('.tweet-card-players');
    if (!list) return;

    if (check.checked) {
        list.querySelectorAll('.tweet-card-player-check').forEach(function (other) {
            if (other !== check) other.checked = false;
        });
    }

    var hidden = document.getElementById(check.getAttribute('data-target'));
    if (hidden) hidden.value = check.checked ? check.getAttribute('data-playerid') : '';

    if (typeof __doPostBack === 'function') {
        __doPostBack(check.getAttribute('data-postback'), '',
                     check.form || check.closest('form'));
    }
});

// Membership promo tabs. Client-side on purpose - a postback to read the next
// tab would feel broken on a marketing panel. Delegated, so several promos can
// sit on one page.
document.addEventListener('click', function (e) {
    var tab = e.target.closest('.membership-promo-tab');
    if (!tab) return;

    var promo = tab.closest('.membership-promo');
    if (!promo) return;

    e.preventDefault();

    promo.querySelectorAll('.membership-promo-tab').forEach(function (t) {
        t.classList.remove('membership-promo-tab--active');
    });
    tab.classList.add('membership-promo-tab--active');

    var targetId = tab.getAttribute('data-promo-target');
    promo.querySelectorAll('.membership-promo-panel').forEach(function (p) {
        p.style.display = (p.id === targetId) ? '' : 'none';
    });
});

(function () {
    function show(panel, index) {
        var track = panel.querySelector('.rotating-panel-track');
        var slides = panel.querySelectorAll('.rotating-panel-slide');
        var dots = panel.querySelectorAll('.rotating-panel-dot');
        if (!track || !slides.length) return;

        var next = ((index % slides.length) + slides.length) % slides.length;

        track.style.transform = 'translateX(-' + (next * 100) + '%)';

        for (var i = 0; i < dots.length; i++) {
            dots[i].classList.toggle('rotating-panel-dot--active', i === next);
        }

        panel._rpIndex = next;
    }

    document.addEventListener('click', function (e) {
        var arrow = e.target.closest('.rotating-panel-arrow');
        if (!arrow) return;

        var panel = arrow.closest('.rotating-panel');
        if (!panel) return;

        e.preventDefault();
        var current = panel._rpIndex || 0;
        show(panel, current + parseInt(arrow.getAttribute('data-rotate'), 10));
        panel._rpPaused = true;
    });

    document.querySelectorAll('.rotating-panel').forEach(function (panel) {
        var seconds = parseInt(panel.getAttribute('data-interval'), 10);
        if (!seconds || seconds < 1) return;

        panel.addEventListener('mouseenter', function () { panel._rpHover = true; });
        panel.addEventListener('mouseleave', function () { panel._rpHover = false; });

        setInterval(function () {
            if (panel._rpHover || panel._rpPaused) return;
            show(panel, (panel._rpIndex || 0) + 1);
        }, seconds * 1000);
    });
})();

document.addEventListener('click', function (e) {
    var pill = e.target.closest('.tweet-card-media-indicator');
    if (!pill) return;

    var card = pill.closest('.tweet-card');
    if (!card) return;

    var media = card.querySelector('.tweet-card-media');
    if (!media) return;

    e.preventDefault();

    var collapsed = media.classList.toggle('tweet-card-media--collapsed');
    pill.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
    pill.setAttribute('title', collapsed ? 'Show media' : 'Hide media');
});

document.addEventListener('input', function (e) {
    if (!e.target.matches('.give-get-picker input[id$="-search"]')) return;

    var baseId = e.target.id.substring(0, e.target.id.length - '-search'.length);
    var dataEl = document.getElementById(baseId + '-data');
    var resultsEl = document.getElementById(baseId + '-results');
    if (!dataEl || !resultsEl) return;

    var query = e.target.value.trim().toLowerCase();
    resultsEl.innerHTML = '';
    if (!query) { resultsEl.style.display = 'none'; return; }

    var players;
    try { players = JSON.parse(dataEl.textContent); } catch (err) { return; }

    var nameMatches = [];
    var aliasMatches = [];
    for (var pi = 0; pi < players.length; pi++) {
        var candidate = players[pi];
        if (candidate.name && candidate.name.toLowerCase().indexOf(query) !== -1) {
            nameMatches.push(candidate);
            continue;
        }
        var aliases = candidate.aliases || [];
        for (var ai = 0; ai < aliases.length; ai++) {
            if (aliases[ai] && aliases[ai].toLowerCase().indexOf(query) !== -1) {
                aliasMatches.push(candidate);
                break;
            }
        }
    }
    var matches = nameMatches.concat(aliasMatches).slice(0, 8);

    if (matches.length === 0) { resultsEl.style.display = 'none'; return; }

    matches.forEach(function (p) {
        var li = document.createElement('li');
        li.className = 'listItem';
        li.textContent = p.name + (p.team ? ' (' + p.team + (p.pos ? ' ' + p.pos : '') + ')' : '');
        li.addEventListener('click', function () {
            var selectedInput = document.getElementById(baseId + '-selected');
            var searchInput = document.getElementById(baseId + '-search');
            if (selectedInput) selectedInput.value = p.id;
            if (searchInput) searchInput.value = p.name;
            resultsEl.innerHTML = '';
            resultsEl.style.display = 'none';
        });
        resultsEl.appendChild(li);
    });

    resultsEl.style.display = 'block';
});

document.addEventListener('click', function (e) {
    var row = e.target.closest('.give-get-picker-search-row');
    document.querySelectorAll('.give-get-picker-results').forEach(function (el) {
        if (row && row.contains(el)) return;
        el.style.display = 'none';
    });
});

document.addEventListener('click', function (e) {
    var toggle = e.target.closest('.tweet-card-players-toggle');
    if (!toggle) return;

    var card = toggle.closest('.tweet-card');
    if (!card) return;

    var players = card.querySelector('.tweet-card-players');
    if (!players) return;

    e.preventDefault();

    var collapsed = players.classList.toggle('tweet-card-players--collapsed');
    toggle.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
    toggle.setAttribute('title', collapsed ? 'Show players' : 'Hide players');
});
(function () {
    function wrapOf(el) {
        return el.closest('.display-columns');
    }

    function isMembership(row) {
        return row.classList.contains('display-columns-item--membership');
    }

    function boxesIn(scope) {
        return Array.prototype.slice.call(
            scope.querySelectorAll('.display-columns-item input[type="checkbox"]')
        );
    }

    function refresh(wrap) {
        var total = 0, on = 0;

        Array.prototype.forEach.call(
            wrap.querySelectorAll('.display-columns-group'),
            function (group) {
                var boxes = boxesIn(group);
                var checked = boxes.filter(function (b) { return b.checked; }).length;

                total += boxes.length;
                on += checked;

                var count = group.querySelector('.display-columns-group-count');
                if (count) count.textContent = checked + '/' + boxes.length;

                var all = group.querySelector('.display-columns-group-all');
                if (all) all.textContent = (boxes.length && checked === boxes.length) ? 'none' : 'all';

                boxes.forEach(function (b) {
                    var row = b.closest('.display-columns-item');
                    if (row) row.classList.toggle('display-columns-item--on', b.checked);
                });
            }
        );

        var summary = wrap.querySelector('[data-dc-count]');
        if (summary) {
            var locked = wrap.querySelectorAll('.display-columns-item--membership').length;
            var text = on + ' of ' + total + ' columns selected';
            if (locked > 0) text += ' \u00b7 ' + locked + ' need a membership';
            summary.textContent = text;
        }
    }

    function filter(wrap, term) {
        var query = (term || '').trim().toLowerCase();
        var anyVisible = false;

        Array.prototype.forEach.call(
            wrap.querySelectorAll('.display-columns-group'),
            function (group) {
                var shown = 0;

                Array.prototype.forEach.call(
                    group.querySelectorAll('.display-columns-item'),
                    function (row) {
                        var label = row.getAttribute('data-dc-label') || '';
                        var hit = !query || label.indexOf(query) !== -1;
                        row.classList.toggle('display-columns-item--hidden', !hit);
                        if (hit) shown++;
                    }
                );

                group.classList.toggle('display-columns-group--hidden', shown === 0);
                if (shown > 0) anyVisible = true;
            }
        );

        var empty = wrap.querySelector('.display-columns-empty');
        if (!empty) {
            empty = document.createElement('div');
            empty.className = 'display-columns-empty';
            var groups = wrap.querySelector('.display-columns-groups');
            if (groups) groups.insertAdjacentElement('afterend', empty);
        }
        empty.textContent = 'Nothing matches "' + query + '"';
        empty.style.display = anyVisible ? 'none' : 'block';
    }

    document.addEventListener('input', function (e) {
        var input = e.target.closest ? e.target.closest('[data-dc-search]') : null;
        if (!input) return;

        var wrap = wrapOf(input);
        if (wrap) filter(wrap, input.value);
    });

    document.addEventListener('change', function (e) {
        if (!e.target.closest) return;
        var row = e.target.closest('.display-columns-item');
        if (!row) return;

        var wrap = wrapOf(row);
        if (wrap) refresh(wrap);
    });

    document.addEventListener('click', function (e) {
        if (!e.target.closest) return;

        if (e.target.closest('.display-columns-tip-wrap') ||
            e.target.closest('.display-columns-lock-wrap')) {
            e.preventDefault();
            return;
        }

        var groupAll = e.target.closest('[data-dc-groupall]');
        if (groupAll) {
            e.preventDefault();
            var group = groupAll.closest('.display-columns-group');
            var boxes = boxesIn(group);
            var allOn = boxes.length > 0 && boxes.every(function (b) { return b.checked; });
            boxes.forEach(function (b) { b.checked = !allOn; });
            refresh(wrapOf(group));
            return;
        }

        var setAll = e.target.closest('[data-dc-setall]');
        if (setAll) {
            e.preventDefault();
            var target = document.getElementById(setAll.getAttribute('data-dc-target'));
            if (!target) return;
            var on = setAll.getAttribute('data-dc-setall') === 'all';
            boxesIn(target).forEach(function (b) { b.checked = on; });
            refresh(target);
        }
    });
})();

// Confirm-before-action. Any element carrying data-bm-confirm prompts first.
// Capture phase, so this runs BEFORE the element's own onclick or form submit.
document.addEventListener('click', function (e) {
    var trigger = e.target.closest && e.target.closest('[data-bm-confirm]');
    if (!trigger) return;

    var message = trigger.getAttribute('data-bm-confirm');
    if (!message) return;

    if (!window.confirm(message)) {
        e.preventDefault();
        e.stopPropagation();
    }
}, true);