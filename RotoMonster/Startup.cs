using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RotoMonster.Core;
using RotoMonster.Core.Services;
using RotoMonster.Data;
using Newtonsoft.Json.Serialization;
using RotoMonsterUI;

namespace RotoMonster
{
    public class Startup
    {
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string sport = Configuration["sport"].ToUpper();

            services.AddDbContextPool<RMDBContext>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("RotoMonsterDb").Replace("{sport}", sport),
                    x => x.MigrationsAssembly("RotoMonster." + sport + ".Migrations"));
                // options.EnableSensitiveDataLogging(true);
                // options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
            );

            services.AddDbContext<RMSharedDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("RotoMonsterSharedDb"),
                    x => x.MigrationsAssembly("RotoMonster")));

            services.AddDefaultIdentity<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount =
                    Configuration.GetValue<bool>("RequireConfirmedAccount", true))
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

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie.Name= "RotoMonster" + sport.ToUpper();
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "RotoMonster" + sport.ToUpper();
            });

            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = "RotoMonsterTempData" + sport.ToUpper());

            //services.AddAntiforgery(options =>
            //{
            //    options.Cookie.Name = options.Cookie.Name.Replace("AspNetCore", "RotoMonster" + sport.ToUpper());
            //});

            services.AddScoped<IRMData, RMSqlData>();
            services.AddScoped<IRMSharedData, RMSharedSqlData>();
            services.AddMemoryCache();
            services.AddRazorPages();
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddTransient<IEmailSender, EmailSender>(i =>
                new EmailSender(
                    Configuration["EmailSender:From"],
                    Configuration["EmailSender:Host"],
                    Configuration.GetValue<int>("EmailSender:Port"),
                    Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                    Configuration["EmailSender:UserName"],
                    Configuration["EmailSender:Password"]
                ));
            // RotoMonsterUI links to Basketball Monster by default.
            RotoMonsterUIUrls.PlayerUrl = id => $"/Players?playerId={id}";

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            if (true || env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            // CreateUserRoles(services).Wait();
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            IdentityResult roleResult;
            //Adding Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck)
            {
                //create the roles and seed them to the database
                roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin"));
            }
            //Assign Admin role to the main User here we have given our newly registered 
            //login id for Admin management
            ApplicationUser user = await UserManager.FindByEmailAsync("kenslight@gmail.com");
            
            var User = new ApplicationUser();
            await UserManager.AddToRoleAsync(user, "Admin");
        }

    }
}
