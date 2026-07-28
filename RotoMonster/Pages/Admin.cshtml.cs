using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using System.Data.SqlClient;
using System.Data;
using RotoMonster.Data;
using Microsoft.Extensions.Logging;

namespace RotoMonster.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRMData db;
        private readonly IRMSharedData sharedData;
        private readonly ILogger<PageModel> logger;

        public List<LogItem> LogItems { get; set; }

        public AdminModel(UserManager<ApplicationUser> userManager, IRMData db, IRMSharedData sharedData, ILogger<PageModel> logger)
        {
            this.userManager = userManager;
            this.db = db;
            this.sharedData = sharedData;
            this.logger = logger;
        }

        public async Task<IActionResult> OnPostClearCache()
        {
            db.ClearCache();
            return Page();
        }

        public async Task<IActionResult> OnPostClearLogs()
        {
            db.ClearLogItems();

            return RedirectToPage("/Admin");
        }

        public async Task<IActionResult> OnPostImportUserAuths()
        {
            int added = 0;
            int updated = 0;

            SqlLib sqlLib = new SqlLib("Data Source=.\\SQL2019; Initial Catalog=R_NBA; Integrated Security=true;");

            // find all BBM users with Yahoo, FanTrax, or ESPN auth
            var bbmAuths = new List<UserAuth>();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM R_NBA..rmUserAuth "
                + " INNER JOIN MonsterUsers..bm_user u ON u.user_id=rmUserAuth.user_id"
                + " WHERE (yahoo_access_token IS NOT NULL AND yahoo_oauth_session_handle IS NOT NULL AND auth_id='B')"
                + " OR (espn_swid IS NOT NULL AND espn_s2 IS NOT NULL)"
                + " OR fantrax_code IS NOT NULL AND fantrax_code NOT LIKE '%@%'"))
            {
                foreach (DataRow row in sqlLib.ExecuteSelect(cmd).Rows)
                {
                    UserAuth bbmAuth;
                    string username = ((string)row["username"]).Trim();
                    bbmAuth = bbmAuths.Find(a => a.BBMUserName == username);
                    if (bbmAuth == null)
                    {
                        bbmAuth = new UserAuth();
                        bbmAuth.BBMUserName = username;
                        bbmAuth.BBMEmail = ((string)row["email"]).Trim();
                        bbmAuths.Add(bbmAuth);
                    }
                    if (row["yahoo_access_token"] != DBNull.Value)
                    {
                        bbmAuth.YahooAccessToken = (string)row["yahoo_access_token"];
                        bbmAuth.YahooRefreshToken = (string)row["yahoo_oauth_session_handle"];
                    }
                    if (row["espn_swid"] != DBNull.Value)
                    {
                        bbmAuth.ESPNswid = (string)row["espn_swid"];
                        bbmAuth.ESPNs2 = (string)row["espn_s2"];
                    }
                    if (row["fantrax_code"] != DBNull.Value)
                    {
                        bbmAuth.FanTraxEmail = (string)row["fantrax_code"];
                    }
                }
            }

            // update existing auths or add new ones
            foreach (var bbmAuth in bbmAuths)
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Id FROM RM_Shared..AspNetUsers WHERE UserName = 'OLD_" + bbmAuth.BBMUserName.Replace("'","''") + "'"))
                {
                    DataTable table=null;
                    try
                    {
                        table= sqlLib.ExecuteSelect(cmd);
                    }
                    catch(Exception ex)
                    {
                        logger.LogInformation("Error with user " + bbmAuth.BBMUserName + " :" + ex.Message);
                    }

                    if (table.Rows.Count == 1)
                    {
                        try
                        {
                            // update existing users
                            bbmAuth.UserId = (string)table.Rows[0]["Id"];
                            using (SqlCommand deleteCmd = new SqlCommand("DELETE RM_Shared..UserAuths WHERE UserId=@UserId"))
                            {
                                deleteCmd.Parameters.AddWithValue("@UserId", bbmAuth.UserId);
                                sqlLib.ExecuteNonSelect(deleteCmd);
                            }

                            sharedData.AddUserAuth(bbmAuth);
                            updated++;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Error updating " + bbmAuth.BBMUserName + " :" + ex.Message);
                        }
                    }
                    else
                    {
                        // add new user
                        try
                        {
                            var user = new ApplicationUser { UserName = "OLD_" + bbmAuth.BBMUserName, Email = "OLD_" + bbmAuth.BBMEmail };
                            var result = await userManager.CreateAsync(user, "x1x1x1");
                            if (result.Succeeded)
                            {
                                bbmAuth.UserId = user.Id;
                                sharedData.AddUserAuth(bbmAuth);
                                added++;
                            }
                            else
                            {
                                logger.LogError("Error creating user " + bbmAuth.BBMUserName);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Error adding " + bbmAuth.BBMUserName + " :" + ex.Message);
                        }
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostImportUsers()
        {
            SqlLib sqlLib = new SqlLib("Data Source=.\\SQL2019; Initial Catalog=R_NBA; Integrated Security=true;");
            SqlCommand cmd = new SqlCommand("SELECT user_id, username, email, password, accesses FROM MonsterUsers..bm_user u"
                + " where date_logged_in >= '1/1/2019' and password<>'' "
                + " and ("
                + " (select count(*) from R_NBA..rmUserMembership um where um.user_id = u.user_id) > 0"
                + " or((select count(*) from R_NBA..rmUserOrderHistory um where um.user_id = u.user_id) > 0))"
                + " ORDER BY username");
            foreach (DataRow row in sqlLib.ExecuteSelect(cmd).Rows)
            {
                int userId = Convert.ToInt32(row["user_id"]);
                string username = Convert.ToString(row["username"]).Trim();
                string email = Convert.ToString(row["email"]).Trim();
                string password = Convert.ToString(row["password"]).Trim();

                var user = new ApplicationUser { UserName = username, Email = email };
                user.EmailConfirmed = true;
                if (await userManager.FindByNameAsync(username) == null)
                {
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded == false)
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine(error.Description);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Done.");
                    }
                }
            }

            return Page();
        }

        public void OnGet(string filterlevel)
        {
            userManager.GetUserId(User);

            LogItems = db.GetLogItems(filterlevel);
        }

    }
}