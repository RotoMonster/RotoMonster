using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class Helper
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } // general purpose of the helper
        public string OnPageDetails { get; set; }   // text to show once the user is redirected by helper
        public string Url { get; set; }
        public bool IsDisabled { get; set; }
        public int DisplayOrder { get; set; }

        public string GetDisplayText(string originalText, PlayerType playerType)
        {
            string txt = originalText;
            if (playerType != null)
            {
                txt = txt.Replace("{playertype_singular}", playerType.SingularTitle);
                txt = txt.Replace("{playertype_plural}", playerType.PluralTitle);
            }

            return txt;
        }

        public string GetPlayerTypeUrl(string originalUrl, PlayerType playerType)
        {
            string url = originalUrl;
            if (playerType != null)
                url = url.Replace("{playertype}", playerType.Id.ToString());

            return url;
        }

    }
}
