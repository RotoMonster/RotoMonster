using System;

namespace RotoMonster.Models.Shared
{
    public class CustomTooltipViewModel
    {
        /// <summary>
        /// Matches the library's id scheme so the shared JS finds it.
        /// </summary>
        public string Id { get; set; } = "bm-tip-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        /// <summary>
        /// Plain text trigger. Encoded on render.
        /// </summary>
        public string TriggerText { get; set; }

        /// <summary>
        /// Pre-rendered trigger markup. Takes precedence over TriggerText and is
        /// emitted raw, so only pass markup this app built.
        /// </summary>
        public string TriggerHtml { get; set; }

        public string ContentText { get; set; }

        /// <summary>
        /// Pre-rendered tooltip body. Takes precedence over ContentText and is
        /// emitted raw, so only pass markup this app built.
        /// </summary>
        public string ContentHtml { get; set; }

        public bool IsCentered { get; set; }

        public bool IsHoverTrigger { get; set; }

        public int? MaxWidth { get; set; }
    }
}
