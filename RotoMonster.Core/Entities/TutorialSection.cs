using System;
using System.ComponentModel.DataAnnotations;

namespace RotoMonster.Core
{
    // One expandable section in the written guide (PageGuide).
    public class TutorialSection
    {
        public int Id { get; set; }
        public int TutorialId { get; set; }

        [Required, StringLength(200)] public string Heading { get; set; }

        // Section content, stored as HTML.
        public string Body { get; set; }

        [StringLength(500)] public string ImageUrl { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsDisabled { get; set; }

        public Tutorial Tutorial { get; set; }
    }
}
