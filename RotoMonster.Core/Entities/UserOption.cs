using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class UserOption
    {
        [Required, StringLength(450)] public string UserId { get; set; }
        public short UserOptionTypeId { get; set; }
        public bool? ValueBool { get; set; }
        public byte? ValueByte { get; set; }
        public short? ValueShort { get; set; }
        public int? ValueInt { get; set; }
        public double? ValueDouble { get; set; }
        public string ValueString { get; set; }

        public UserOptionType UserOptionType { get; set; }
    }
}
