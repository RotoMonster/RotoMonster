using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserInvitation
    {
        public int Id { get; set; }
        public string InvitationID { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUsed { get; set; }
        public string BBMUsername { get; set; }
        public string UserIdCreated { get; set; }
    }

}
