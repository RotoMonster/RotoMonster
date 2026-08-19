using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RotoMonster.Core
{
    public class Tutorial
    {
        public int Id { get; set; }

        // Lookup slug the page passes in, e.g. "rankings" or "rankings-punt".
        // A page may request more than one, so this is not one-per-page.
        [Required, StringLength(64)] public string TutorialKey { get; set; }

        [Required, StringLength(200)] public string Title { get; set; }

        // The "why this page exists" paragraph. Null for walkthrough-only tutorials.
        public string Purpose { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime ModifiedUtc { get; set; }

        public ICollection<TutorialSection> TutorialSections { get; set; }
        public ICollection<TutorialStep> TutorialSteps { get; set; }
    }
}
