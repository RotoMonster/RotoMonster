using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class UserDisplayColumns
    {
        public List<DisplayColumn> DisplayColumns { get; set; } = new List<DisplayColumn>();
        public bool DisplayAll { get; set; } = false;
        public bool IsSelected(string column)
        {
            if (DisplayAll)
                return true;

            return (from dc in DisplayColumns
                    where dc.UserOptionType.Title.ToLower().Replace(" ","") == column.ToLower().Replace(" ", "")
                    && dc.IsSelected select dc.IsSelected).FirstOrDefault(false);
        }
    }
}
