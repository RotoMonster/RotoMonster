using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace RotoMonster.Core
{
    public class Player
    {
        public int Id { get; set; }

        [Display(Name = "First Name")]
        [Required, StringLength(80)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required, StringLength(80)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        [Required, Range(50, 100)]
        [Display(Name = "Height (in)")]
        public int Height { get; set; }

        [Required, Range(100, 500)]
        [Display(Name = "Weight (lb)")]
        public int Weight { get; set; }

        [Display(Name = "Rookie Year")]
        public int? RookieYear { get; set; }
        [Display(Name = "Pick #")]
        public int? PickNumber { get; set; }

        [StringLength(1)]
        public string Bats { get; set; }
        [StringLength(1)]
        public string Throws { get; set; }

        public List<SeasonPlayer> SeasonPlayers { get; set; }
        public List<PlayerDefaultPosition> PlayerDefaultPositions { get; set; }

        public string ForwardName
        {
            get
            {
                return (FirstName + " " + LastName).Trim();
            }
        }

        public string BriefName
        {
            get
            {
                string o = "";
                if (FirstName.Length > 0)
                    o = FirstName.Substring(0, 1);
                o += " " + LastName;

                return o.Trim();
            }
        }

        public Position DefaultPosition
        {
            get
            {
                if (PlayerDefaultPositions == null)
                    return null;

                var dp = (from p in PlayerDefaultPositions where p.Position.IsActualPosition orderby p.Position.DisplayOrder select p).FirstOrDefault();

                return (dp != null ? dp.Position : null);
            }
        }

    }
}
