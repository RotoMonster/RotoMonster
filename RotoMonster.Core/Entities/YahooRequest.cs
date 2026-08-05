using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class YahooRequest
    {
        public int Id { get; set; }
        public DateTime DateAdded { get; set; }
        public string UserId { get; set; }
        public string ProcessorId { get; set; }
        public string Url { get; set; }
        public string Results { get; set; }
        public DateTime? DateProcessed { get; set; }
        public string ErrorMessage { get; set; } = "";

        public bool WasSuccessful
        {
            get
            {
                if (Results == null)
                    return false;

                return Results.Length > 0;
            }
        }

    }
}
