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
            this.CBSPid = "";
            this.FanTraxEmail = "";
            this.SleeperName = "";
            this.SleeperId = "";
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

        /// <summary>
        /// The CBS pid cookie, which is the whole of their auth. Store
        /// it exactly as CBS gives it - a URL encoded copy is rejected
        /// and every request lands on the login page.
        /// </summary>
        [StringLength(200)]
        public string CBSPid { get; set; }
        [StringLength(100)]
        public string FanTraxEmail { get; set; }

        /// <summary>
        /// The username the user types in. Kept so it can be shown back
        /// to them, since the id below means nothing to a person.
        /// </summary>
        [StringLength(100)]
        public string SleeperName { get; set; }

        /// <summary>
        /// Sleeper's own user id, looked up once from the username. This
        /// is what every Sleeper call is keyed on.
        /// </summary>
        [StringLength(50)]
        public string SleeperId { get; set; }

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
                if (!string.IsNullOrEmpty(SleeperId) && SleeperId.Trim().Length > 0)
                    return true;
                if (!string.IsNullOrEmpty(CBSPid) && CBSPid.Trim().Length > 0)
                    return true;

                return false;
            }
        }


    }

}
