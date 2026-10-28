using System;

namespace RotoMonster.Core
{
    public class AdminTask
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public string Url { get; set; }
        public string UrlText { get; set; }
        public bool IsAdminOnly { get; set; }
        public string Source { get; set; }
        public string DedupeKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DoneAt { get; set; }
        public string DoneBy { get; set; }
        public string ActionType { get; set; }
        public string ActionLabel { get; set; }
        public string ActionJson { get; set; }
        public DateTime? ActionRanAt { get; set; }
        public string ActionResult { get; set; }
    }
}
