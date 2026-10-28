using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;

namespace RotoMonster.Pages
{
    [Authorize(Roles = "Admin,Worker")]
    public class AdminTasksModel : PageModel
    {
        private readonly AdminTaskService _tasks;
        private readonly IEnumerable<IAdminTaskAction> _actions;
        private readonly ILogger<AdminTasksModel> _log;

        public AdminTasksModel(AdminTaskService tasks, IEnumerable<IAdminTaskAction> actions, ILogger<AdminTasksModel> log)
        {
            _tasks = tasks;
            _actions = actions;
            _log = log;
        }

        public List<AdminTask> Tasks { get; set; } = new List<AdminTask>();
        public AdminDutyState Duty { get; set; } = new AdminDutyState();
        public bool ViewAll { get; set; }
        public bool IsAdmin => User.IsInRole("Admin");
        public string Me => User.Identity?.Name ?? "";

        public void OnGet(bool viewAll = false)
        {
            ViewData["PageHeading"] = "Admin Tasks";
            Load(viewAll);
        }

        public IActionResult OnGetList(bool viewAll = false)
        {
            return List(viewAll);
        }

        public IActionResult OnPostDone(int id, bool viewAll = false)
        {
            _tasks.MarkDone(id, Me, IsAdmin);
            return List(viewAll);
        }

        public IActionResult OnPostUndo(int id, bool viewAll = false)
        {
            _tasks.Undo(id, IsAdmin);
            return List(viewAll);
        }

        public IActionResult OnPostDoneAll(bool viewAll = false)
        {
            _tasks.MarkAllDone(Me, false, IsAdmin);
            return List(viewAll);
        }

        public IActionResult OnPostDoneAllAdmin(bool viewAll = false)
        {
            if (!IsAdmin) return Forbid();
            _tasks.MarkAllDone(Me, true, true);
            return List(viewAll);
        }

        public IActionResult OnPostReady(bool viewAll = false)
        {
            _tasks.Ready(Me);
            return List(viewAll);
        }

        public IActionResult OnPostOnDuty(bool viewAll = false)
        {
            _tasks.OnDuty(Me);
            return List(viewAll);
        }

        public IActionResult OnPostOffDuty(bool viewAll = false)
        {
            _tasks.OffDuty(Me);
            return List(viewAll);
        }

        public IActionResult OnPostAuto(bool auto, bool viewAll = false)
        {
            _tasks.SetAuto(Me, auto);
            return List(viewAll);
        }

        public async Task<IActionResult> OnPostRunAsync(int id, bool viewAll = false)
        {
            var task = _tasks.Get(id);
            if (task == null || task.DoneAt != null || string.IsNullOrEmpty(task.ActionType))
                return List(viewAll);
            if (task.IsAdminOnly && !IsAdmin)
                return Forbid();

            var action = _actions.FirstOrDefault(a => string.Equals(a.ActionType, task.ActionType, StringComparison.OrdinalIgnoreCase));
            if (action == null)
            {
                _tasks.RecordAction(id, "No action set up for " + task.ActionType, Me, false);
                return List(viewAll);
            }

            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(task.ActionJson) ? "{}" : task.ActionJson);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                var result = await action.RunAsync(task, doc.RootElement, cts.Token);
                _tasks.RecordAction(id, string.IsNullOrEmpty(result) ? "Done" : result, Me, true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Admin task action {ActionType} failed for task {Id}", task.ActionType, id);
                _tasks.RecordAction(id, "Failed: " + ex.Message, Me, false);
            }

            return List(viewAll);
        }

        public static string Ago(DateTime utc)
        {
            var span = DateTime.UtcNow - utc;
            if (span.TotalMinutes < 1) return "now";
            if (span.TotalMinutes < 60) return (int)span.TotalMinutes + "m";
            if (span.TotalHours < 24) return (int)span.TotalHours + "h";
            return (int)span.TotalDays + "d";
        }

        public static string ShortName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return "";
            var at = userName.IndexOf('@');
            return at > 0 ? userName.Substring(0, at) : userName;
        }

        private void Load(bool viewAll)
        {
            ViewAll = viewAll;
            Tasks = _tasks.GetTasks(viewAll, IsAdmin);
            Duty = _tasks.GetDuty(Me);
        }

        private IActionResult List(bool viewAll)
        {
            Load(viewAll);
            return Partial("_AdminTasksList", this);
        }
    }
}
