using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;

namespace RotoMonster.Data
{
    public interface IAdminTaskAction
    {
        string ActionType { get; }
        Task<string> RunAsync(AdminTask task, JsonElement data, CancellationToken ct);
    }

    public class AdminDutyState
    {
        public string OnDuty { get; set; }
        public List<string> Ready { get; set; } = new List<string>();
        public bool MyAuto { get; set; }
        public bool IAmOnDuty { get; set; }
        public bool IAmReady { get; set; }
    }

    public class AdminTaskService
    {
        public static readonly TimeSpan ViewAllDays = TimeSpan.FromDays(7);

        private readonly RMDBContext _db;

        public AdminTaskService(RMDBContext db)
        {
            _db = db;
        }

        public List<AdminTask> GetTasks(bool includeDone, bool includeAdminOnly)
        {
            var q = _db.AdminTasks.AsNoTracking().AsQueryable();
            if (!includeAdminOnly)
                q = q.Where(t => !t.IsAdminOnly);

            if (includeDone)
            {
                var since = DateTime.UtcNow - ViewAllDays;
                q = q.Where(t => t.DoneAt == null || t.DoneAt >= since);
            }
            else
            {
                q = q.Where(t => t.DoneAt == null);
            }

            return q.OrderByDescending(t => t.CreatedAt).Take(300).ToList();
        }

        public AdminTask Get(int id)
        {
            return _db.AdminTasks.AsNoTracking().FirstOrDefault(t => t.Id == id);
        }

        public AdminTask Add(AdminTask task)
        {
            if (!string.IsNullOrEmpty(task.DedupeKey))
            {
                var open = _db.AdminTasks.AsNoTracking().FirstOrDefault(t => t.DoneAt == null && t.DedupeKey == task.DedupeKey);
                if (open != null)
                    return open;
            }

            task.Id = 0;
            task.CreatedAt = DateTime.UtcNow;
            task.DoneAt = null;
            task.DoneBy = null;
            task.ActionRanAt = null;
            task.ActionResult = null;

            _db.AdminTasks.Add(task);
            _db.SaveChanges();
            return task;
        }

        public bool MarkDone(int id, string userName, bool canSeeAdmin)
        {
            var task = _db.AdminTasks.FirstOrDefault(t => t.Id == id && t.DoneAt == null);
            if (task == null || (task.IsAdminOnly && !canSeeAdmin))
                return false;

            task.DoneAt = DateTime.UtcNow;
            task.DoneBy = userName;
            _db.SaveChanges();
            return true;
        }

        public bool Undo(int id, bool canSeeAdmin)
        {
            var task = _db.AdminTasks.FirstOrDefault(t => t.Id == id && t.DoneAt != null);
            if (task == null || (task.IsAdminOnly && !canSeeAdmin))
                return false;

            task.DoneAt = null;
            task.DoneBy = null;
            _db.SaveChanges();
            return true;
        }

        public int MarkDoneByKey(string dedupeKey, string doneBy)
        {
            if (string.IsNullOrEmpty(dedupeKey))
                return 0;

            var tasks = _db.AdminTasks.Where(t => t.DoneAt == null && t.DedupeKey == dedupeKey).ToList();
            var now = DateTime.UtcNow;
            foreach (var t in tasks)
            {
                t.DoneAt = now;
                t.DoneBy = doneBy;
            }
            _db.SaveChanges();
            return tasks.Count;
        }

        public int MarkAllDone(string userName, bool adminOnly, bool canSeeAdmin)
        {
            var q = _db.AdminTasks.Where(t => t.DoneAt == null);
            if (adminOnly)
            {
                if (!canSeeAdmin)
                    return 0;
                q = q.Where(t => t.IsAdminOnly);
            }
            else if (!canSeeAdmin)
            {
                q = q.Where(t => !t.IsAdminOnly);
            }

            var tasks = q.ToList();
            var now = DateTime.UtcNow;
            foreach (var t in tasks)
            {
                t.DoneAt = now;
                t.DoneBy = userName;
            }
            _db.SaveChanges();
            return tasks.Count;
        }

        public void RecordAction(int id, string result, string userName, bool markDone)
        {
            var task = _db.AdminTasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
                return;

            task.ActionRanAt = DateTime.UtcNow;
            task.ActionResult = result != null && result.Length > 500 ? result.Substring(0, 500) : result;
            if (markDone && task.DoneAt == null)
            {
                task.DoneAt = task.ActionRanAt;
                task.DoneBy = userName;
            }
            _db.SaveChanges();
        }

        public AdminDutyState GetDuty(string userName)
        {
            var rows = _db.AdminDuties.AsNoTracking().ToList();
            var me = rows.FirstOrDefault(r => r.UserName == userName);
            return new AdminDutyState
            {
                OnDuty = rows.FirstOrDefault(r => r.IsOnDuty)?.UserName,
                Ready = rows.Where(r => r.ReadyAt != null && !r.IsOnDuty).OrderBy(r => r.ReadyAt).Select(r => r.UserName).ToList(),
                MyAuto = me != null && me.Auto,
                IAmOnDuty = me != null && me.IsOnDuty,
                IAmReady = me != null && me.ReadyAt != null && !me.IsOnDuty
            };
        }

        public void Ready(string userName)
        {
            var me = Row(userName);
            if (me.IsOnDuty)
                return;

            var current = _db.AdminDuties.FirstOrDefault(d => d.IsOnDuty);
            if (current == null || current.Auto)
            {
                TakeOver(me);
            }
            else
            {
                me.ReadyAt = DateTime.UtcNow;
                me.UpdatedAt = DateTime.UtcNow;
            }
            _db.SaveChanges();
        }

        public void OnDuty(string userName)
        {
            TakeOver(Row(userName));
            _db.SaveChanges();
        }

        public void OffDuty(string userName)
        {
            var me = Row(userName);
            me.ReadyAt = null;
            me.UpdatedAt = DateTime.UtcNow;

            if (me.IsOnDuty)
            {
                me.IsOnDuty = false;
                var next = _db.AdminDuties.Where(d => d.ReadyAt != null && d.Id != me.Id).OrderBy(d => d.ReadyAt).FirstOrDefault();
                if (next != null)
                {
                    next.IsOnDuty = true;
                    next.ReadyAt = null;
                    next.UpdatedAt = DateTime.UtcNow;
                }
            }
            _db.SaveChanges();
        }

        public void SetAuto(string userName, bool auto)
        {
            var me = Row(userName);
            me.Auto = auto;
            me.UpdatedAt = DateTime.UtcNow;

            if (auto && me.IsOnDuty)
            {
                var next = _db.AdminDuties.Where(d => d.ReadyAt != null && d.Id != me.Id).OrderBy(d => d.ReadyAt).FirstOrDefault();
                if (next != null)
                    TakeOver(next);
            }
            _db.SaveChanges();
        }

        private void TakeOver(AdminDuty me)
        {
            var now = DateTime.UtcNow;
            foreach (var d in _db.AdminDuties.Where(d => d.IsOnDuty && d.Id != me.Id).ToList())
            {
                d.IsOnDuty = false;
                d.UpdatedAt = now;
            }
            me.IsOnDuty = true;
            me.ReadyAt = null;
            me.UpdatedAt = now;
        }

        private AdminDuty Row(string userName)
        {
            var row = _db.AdminDuties.FirstOrDefault(d => d.UserName == userName);
            if (row == null)
            {
                row = new AdminDuty { UserName = userName, UpdatedAt = DateTime.UtcNow };
                _db.AdminDuties.Add(row);
                _db.SaveChanges();
            }
            return row;
        }
    }
}
