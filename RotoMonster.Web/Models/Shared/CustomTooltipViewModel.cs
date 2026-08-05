using System;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class CustomTooltipViewModel
    {
        /// <summary>
        /// Matches the library's id scheme so the shared JS finds it.
        /// </summary>
        private string _id;

        public string Id
        {
            get { return _id ?? (_id = "bm-tip-" + StableKey()); }
            set { _id = value; }
        }

        private string StableKey()
        {
            var basis = (TriggerHtml ?? TriggerText ?? "") + "|" + (ContentHtml ?? ContentText ?? "");
            unchecked
            {
                uint hash = 2166136261;
                foreach (var c in basis)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return hash.ToString("x8");
            }
        }

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
