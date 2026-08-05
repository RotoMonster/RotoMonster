using Microsoft.AspNetCore.Http;

namespace RotoMonster.Models.Shared
{
    public static class TooltipIds
    {
        private const string Key = "rm-tooltip-seq";

        public static string NextTooltipId(this HttpContext context)
        {
            var n = context.Items[Key] as int? ?? 0;
            n++;
            context.Items[Key] = n;
            return "bm-tip-" + n;
        }
    }
}
