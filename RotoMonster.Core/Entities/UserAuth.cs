using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class UserAuth
    {
        public UserAuth()
        {
            this.YahooAccessToken = "";
            this.YahooRefreshToken = "";
            this.ESPNInfo = "";
            this.ESPNs2 = "";
            this.ESPNswid = "";
            this.CBSUsername = "";
            this.CBSPassword = "";
            this.FanTraxEmail = "";
        }

        [StringLength(260)]
        public string UserId { get; set; }
        [StringLength(10)]
        public DateTime DateAdded { get; set; }
        public DateTime LastUsed { get; set; }
        public bool HasBeenUsed { get; set; }
        [StringLength(1500)]
        public string YahooAccessToken { get; set; }
        [StringLength(1500)]
        public string YahooRefreshToken { get; set; }
        [StringLength(500)]
        public string ESPNswid { get; set; }
        [StringLength(500)]
        public string ESPNs2 { get; set; }
        [StringLength(500)]
        public string ESPNInfo { get; set; }
        [StringLength(100)]
        public string CBSUsername { get; set; }
        [StringLength(100)]
        public string CBSPassword { get; set; }
        [StringLength(100)]
        public string FanTraxEmail { get; set; }

        [NotMapped] public string BBMUserName { get; set; } = "";
        [NotMapped] public string BBMEmail { get; set; } = "";
 
        [NotMapped]
        public TimeSpan TokenAge
        {
            get
            {
                return DateTime.UtcNow - LastUsed;
            }
        }

        [NotMapped]
        public bool MustRefreshYahoo
        {
            get
            {
                return TokenAge.TotalMinutes > 55;
            }
        }

        [NotMapped]
        public bool HasAuth
        {
            get
            {
                if (YahooAccessToken.Trim().Length > 0 && YahooRefreshToken.Trim().Length > 0)
                    return true;
                if (ESPNswid.Trim().Length > 0 && ESPNs2.Trim().Length > 0)
                    return true;
                if (FanTraxEmail.Trim().Length > 0)
                    return true;

                return false;
            }
        }


    }

}
