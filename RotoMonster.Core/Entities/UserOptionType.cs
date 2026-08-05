using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class UserOptionType
    {
        public short Id { get; set; }
        
        [StringLength(50)]
        public string Title { get; set; }
        
        [StringLength(20)]
        public string Abbreviation { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string OptionGroup { get; set; }

        public bool IsEnabled { get; set; }

        public short DisplayOrder { get; set; }
        [StringLength(1)] public string DataType { get; set; }

        public bool? DefaultValueBool { get; set; }
        public byte? DefaultValueByte { get; set; }
        public short? DefaultValueShort { get; set; }
        public int? DefaultValueInt { get; set; }
        public double? DefaultValueDouble { get; set; }
        public string DefaultValueString { get; set; }

    }
}
