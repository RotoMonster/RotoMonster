using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class MonsterBotPlayerComment
    {
        public string Text { get; set; } = "";
        public bool IsOK { get; set; } = false;
        public bool IsAlert { get; set; } = false;
        public bool IsWarning { get; set; } = false;
        public string Icon { get; set; } = "";

        //public string Icon
        //{
        //    get
        //    {
        //        if (IsAlert)
        //            return "fas fa-exclamation-triangle";
        //        else if (IsWarning)
        //            return "fas fa-question-circle";
        //        else
        //            return "fas fa-check-circle";
        //    }
        //}

        public string Color
        {
            get
            {
                if (IsAlert)
                    return "text-danger";            
                else if (IsWarning)
                    return "text-warning";
                else
                    return "text-success";
            }
        }

    }

}
