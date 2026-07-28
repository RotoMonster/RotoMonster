using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders.Testing;
using RotoMonster.Core;
using RotoMonster.Core.Libs;

namespace RotoMonster.Data
{
    public class RMSharedDbContext : IdentityDbContext<ApplicationUser>
    {
        public RMSharedDbContext(DbContextOptions<RMSharedDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserAuth> UserAuths { get; set; }
        public DbSet<YahooRequest> YahooRequests { get; set; }
        public DbSet<UserInvitation> UserInvitations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<UserAuth>().HasKey(s => new { s.UserId });
            builder.Entity<UserAuth>().ToTable("UserAuths");

        }

    }
}
