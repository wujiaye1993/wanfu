using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenuTest1
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            // Configuration and settings menus for the AdminMenu module
            builder.Add(S["设置"],"1", cfg => cfg
                    .Add(S["公众号设置"], "2", wcmp => wcmp
                        .Action("List", "WeChatMP", new { area = "OrchardCore.AdminMenuTest1" })
                        .LocalNav()
                    )
                    .Add(S["客服设置"], "3", kf => kf
                        .Action("List", "Kefu", new { area = "OrchardCore.AdminMenuTest1" })
                        .LocalNav()
                    ));
            return Task.CompletedTask;
            //// This is the entry point for the adminMenu: dynamically generated custom admin menus
            //await _adminMenuNavigationProvider.BuildNavigationAsync("adminMenu", builder);
        }
    }
}
