using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.GitHub.Settings;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Services
{
    public class GitHubAuthenticationService : IGitHubAuthenticationService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<GitHubAuthenticationService> T;
        private readonly ShellSettings _shellSettings;

        public GitHubAuthenticationService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<GitHubAuthenticationService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<GitHubAuthenticationSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<GitHubAuthenticationSettings>();
        }

        public async Task UpdateSettingsAsync(GitHubAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.GetSiteSettingsAsync();
            container.Alter<GitHubAuthenticationSettings>(nameof(GitHubAuthenticationSettings), aspect =>
            {
                aspect.ClientID = settings.ClientID;
                aspect.ClientSecret = settings.ClientSecret;
                aspect.CallbackPath = settings.CallbackPath;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(GitHubAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.ClientID))
            {
                yield return new ValidationResult(T["ClientID is required"], new string[] { nameof(settings.ClientID) });
            }

            if (string.IsNullOrWhiteSpace(settings.ClientSecret))
            {
                yield return new ValidationResult(T["ClientSecret is required"], new string[] { nameof(settings.ClientSecret) });
            }
        }

    }
}
