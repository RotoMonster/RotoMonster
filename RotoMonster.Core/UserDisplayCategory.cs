using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RotoMonster.Core
{
    public class UserDisplayCategory
    {
        [StringLength(260)]
        public string UserId { get; set; }

        public int CategoryId { get; set; }
        public int DisplayOrder { get; set; }

        public Category Category { get; set; }
    }

}
