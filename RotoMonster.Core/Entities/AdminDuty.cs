using System;

namespace RotoMonster.Core
{
    public class AdminDuty
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public bool IsOnDuty { get; set; }
        public DateTime? ReadyAt { get; set; }
        public bool Auto { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
