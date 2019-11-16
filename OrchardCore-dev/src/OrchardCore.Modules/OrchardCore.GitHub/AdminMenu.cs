using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.GitHub
{
    [Feature(GitHubConstants.Features.GitHubAuthentication)]
    public class AdminMenuGitHubLogin : INavigationProvider
    {
        private readonly ShellDescriptor _shellDescriptor;

        public AdminMenuGitHubLogin(
            IStringLocalizer<AdminMenuGitHubLogin> localizer,
            ShellDescriptor shellDescriptor)
        {
            T = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(T["GitHub"], "15", settings => settings
                        .AddClass("github").Id("github")
                        .Add(T["GitHub Authentication"], "10", client => client
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GitHubConstants.Features.GitHubAuthentication })
                            .Permission(Permissions.ManageGitHubAuthentication)
                            .LocalNav())
                    );
            }
            return Task.CompletedTask;
        }
    }
}
