using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media
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

            builder
                .Add(S["Content"], content => content
                    .Add(S["Assets"], "3", layers => layers
                        .Permission(Permissions.ManageOwnMedia)
                        .Action("Index", "Admin", new { area = "OrchardCore.Media" })
                        .LocalNav()
                    ));

            return Task.CompletedTask;
        }
    }

    public class MediaCacheAdminMenu : INavigationProvider
    {
        public MediaCacheAdminMenu(IStringLocalizer<AdminMenu> localizer)
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

            builder.Add(S["Configuration"], content => content
                .Add(S["Asset Cache"], "1", contentItems => contentItems
                    .Action("Index", "MediaCache", new { area = "OrchardCore.Media" })
                    .Permission(MediaCachePermissions.ManageAssetCache)
                    .LocalNav())
                );

            return Task.CompletedTask;
        }
    }
}
