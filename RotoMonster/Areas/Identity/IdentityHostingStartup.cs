using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RotoMonster.Core;
using RotoMonster.Data;

[assembly: HostingStartup(typeof(RotoMonster.Areas.Identity.IdentityHostingStartup))]
namespace RotoMonster.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<RMSharedDbContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("RotoMonsterSharedDb"), x => x.MigrationsAssembly("RotoMonster")));

                services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<RMSharedDbContext>();

                services.Configure<IISServerOptions>(options =>
                {
                    options.AutomaticAuthentication = false;
                });

                services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 1;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.AllowedUserNameCharacters += " !~$#'&";
                });
            });
        }
    }
}