using System;
using System.ComponentModel.DataAnnotations;

namespace RotoMonster.Core
{
    // One spotlight stop in the interactive walkthrough (Tour / TourStep).
    public class TutorialStep
    {
        public int Id { get; set; }
        public int TutorialId { get; set; }

        // CSS selector the pulse ring points at.
        [Required, StringLength(200)] public string TargetSelector { get; set; }

        [StringLength(200)] public string Title { get; set; }
        public string Body { get; set; }

        // top / bottom / left / right / auto
        [StringLength(10)] public string Placement { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsDisabled { get; set; }

        public Tutorial Tutorial { get; set; }
    }
}
