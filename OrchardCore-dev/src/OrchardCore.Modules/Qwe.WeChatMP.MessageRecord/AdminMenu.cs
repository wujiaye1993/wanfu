using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace Qwe.WeChatMP.MessageRecord
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
            builder.Add(S["历史记录"],"1", cfg => cfg
                    
                        .Action("List", "Home", new { area = "Qwe.WeChatMP.MessageRecord" })
                        .LocalNav()
                    
                   
                    );
            return Task.CompletedTask;
            //// This is the entry point for the adminMenu: dynamically generated custom admin menus
            //await _adminMenuNavigationProvider.BuildNavigationAsync("adminMenu", builder);
        }
    }
}
