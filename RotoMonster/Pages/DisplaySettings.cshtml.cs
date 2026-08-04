using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;

namespace RotoMonster.Pages
{
    [Authorize]
    public class DisplaySettingsModel : RMPageModel
    {
        [BindProperty]
        public DisplayCategorySelect[] DisplayCategorySelects { get; set; }

        [BindProperty]
        public DisplayColumnSelect[] DisplayColumnSelects { get; set; }

        public DisplaySettingsModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public async Task OnGetAsync()
        {
            InitGet("Display Settings");

            var displayCategories = db.GetDisplayCategories();
            var userDisplayCategories = await db.GetUserDisplayCategoriesAsync(UserId, null);

            if (userDisplayCategories != null)
            {
                DisplayCategorySelects = new DisplayCategorySelect[displayCategories.Count];
                for (int i = 0; i < DisplayCategorySelects.Count(); i++)
                {
                    var displayCategorySelect = new DisplayCategorySelect();
                    DisplayCategorySelects[i] = displayCategorySelect;
                    displayCategorySelect.Category = (Category)displayCategories[i];
                    displayCategorySelect.Id = displayCategorySelect.Category.Id;
                    displayCategorySelect.Name = displayCategorySelect.Category.DisplayTitle;
                    displayCategorySelect.Selected = ((from c in userDisplayCategories where c.CategoryId == displayCategorySelect.Category.Id select c).FirstOrDefault() != null);
                }
            }

            var displayColumns = await db.GetDisplayColumnsAsync(UserId);
            DisplayColumnSelects = new DisplayColumnSelect[displayColumns.Count];
            for (int i = 0; i < DisplayColumnSelects.Count(); i++)
            {
                var displayColumnSelect = new DisplayColumnSelect();
                DisplayColumnSelects[i] = displayColumnSelect;
                displayColumnSelect.Id = displayColumns[i].UserOptionType.Id;
                displayColumnSelect.Name = displayColumns[i].UserOptionType.Title;
                displayColumnSelect.Selected = displayColumns[i].IsSelected;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userDisplayCategories = new List<UserDisplayCategory>();
            foreach (var selectDisplayCategory in DisplayCategorySelects)
            {
                if (selectDisplayCategory.Selected)
                {
                    var userDisplayCategory = new UserDisplayCategory();
                    userDisplayCategory.UserId = UserId;
                    userDisplayCategory.CategoryId = selectDisplayCategory.Id; ;
                    userDisplayCategory.DisplayOrder = 1;
                    userDisplayCategories.Add(userDisplayCategory);
                }
            }
            await db.UpdateUserDisplayCategoriesAsync(UserId, userDisplayCategories);

            var inDisplayColumns = await db.GetDisplayColumnsAsync(UserId);
            var outDisplayColumns = new List<DisplayColumn>();
            foreach (var selectDisplayColumn in DisplayColumnSelects)
            {
                var displayColumn = new DisplayColumn();
                displayColumn.UserOptionType = (from d in inDisplayColumns where d.UserOptionType.Id == selectDisplayColumn.Id select d.UserOptionType).FirstOrDefault();
                displayColumn.IsSelected = selectDisplayColumn.Selected;
                outDisplayColumns.Add(displayColumn);
            }
            await db.UpdateDisplayColumnsAsync(UserId, outDisplayColumns);

            AddMessage("Your settings have been saved.");

            return Page();
        }

    }

    public class DisplayCategorySelect
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public Category Category { get; set; }
    }

    public class DisplayColumnSelect
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}