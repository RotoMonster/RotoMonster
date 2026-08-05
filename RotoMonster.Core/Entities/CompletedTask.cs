using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class CompletedTask
    {
        public int Id { get; set; }
        public string TaskId { get; set; } = "";
        public DateTime DateCompleted { get; set; }
        public bool WasSuccess { get; set; }
    }
}
