using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<string> _configuredTenants;
        private readonly Func<string, IConfigurationBuilder> _configBuilderFactory;
        private readonly IShellConfigurationSources _tenantConfigSources;
        private readonly IShellsSettingsSources _settingsSources;

        public ShellSettingsManager(
            IConfiguration applicationConfiguration,
            IShellsConfigurationSources configurationSources,
            IShellConfigurationSources tenantConfigSources,
            IShellsSettingsSources settingsSources,
            IOptions<ShellOptions> options)
        {
            _tenantConfigSources = tenantConfigSources;
            _settingsSources = settingsSources;

            var lastProviders = (applicationConfiguration as IConfigurationRoot)?.Providers
                .Where(p => p is EnvironmentVariablesConfigurationProvider ||
                            p is CommandLineConfigurationProvider)
                .ToArray();

            var configurationBuilder = new ConfigurationBuilder()
                .AddConfiguration(applicationConfiguration)
                .AddSources(configurationSources);

            if (lastProviders.Count() > 0)
            {
                configurationBuilder.AddConfiguration(new ConfigurationRoot(lastProviders));
            }

            _configuration = configurationBuilder.Build().GetSection("OrchardCore");

            _configuredTenants = _configuration.GetChildren()
                .Where(section => section["State"] != null)
                .Select(section => section.Key)
                .Distinct()
                .ToArray();

            _configBuilderFactory = (tenant) =>
            {
                var builder = new ConfigurationBuilder().AddConfiguration(_configuration);

                if (_configuredTenants.Contains(tenant))
                {
                    builder.AddConfiguration(_configuration.GetSection(tenant));
                }

                return builder.AddSources(tenant, _tenantConfigSources);
            };
        }

        public ShellSettings CreateDefaultSettings()
        {
            return new ShellSettings
            (
                new ShellConfiguration(_configuration),
                new ShellConfiguration(_configuration)
            );
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var tenantsSettings = new ConfigurationBuilder()
                .AddSources(_settingsSources)
                .Build();

            var tenants = tenantsSettings.GetChildren().Select(section => section.Key);
            
            var allTenants = _configuredTenants.Concat(tenants).Distinct().ToArray();

            var allSettings = new List<ShellSettings>();

            foreach (var tenant in allTenants)
            {
                var tenantSettings = new ConfigurationBuilder()
                    .AddConfiguration(_configuration)
                    .AddConfiguration(_configuration.GetSection(tenant))
                    .AddConfiguration(tenantsSettings.GetSection(tenant))
                    .Build();

                var settings = new ShellConfiguration(tenantSettings);
                var configuration = new ShellConfiguration(tenant, _configBuilderFactory);

                var shellSettings = new ShellSettings(settings, configuration)
                {
                    Name = tenant,
                };

                allSettings.Add(shellSettings);
            };

            return allSettings;
        }

        public void SaveSettings(ShellSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var configuration = new ConfigurationBuilder()
                .AddConfiguration(_configuration)
                .AddConfiguration(_configuration.GetSection(settings.Name))
                .Build();

            var shellSettings = new ShellSettings()
            {
                Name = settings.Name
            };

            configuration.Bind(shellSettings);

            var configSettings = JObject.FromObject(shellSettings);
            var tenantSettings = JObject.FromObject(settings);

            foreach (var property in configSettings)
            {
                var tenantValue = tenantSettings.Value<string>(property.Key);
                var configValue = configSettings.Value<string>(property.Key);

                if (tenantValue != configValue)
                {
                    tenantSettings[property.Key] = tenantValue;
                }
                else
                {
                    tenantSettings[property.Key] = null;
                }
            }

            tenantSettings.Remove("Name");

            _settingsSources.Save(settings.Name, tenantSettings.ToObject<Dictionary<string, string>>());

            var tenantConfig = new JObject();

            var sections = settings.ShellConfiguration.GetChildren()
                .Where(s => !s.GetChildren().Any())
                .ToArray();

            foreach (var section in sections)
            {
                if (settings[section.Key] != configuration[section.Key])
                {
                    tenantConfig[section.Key] = settings[section.Key];
                }
                else
                {
                    tenantConfig[section.Key] = null;
                }
            }

            tenantConfig.Remove("Name");

            _tenantConfigSources.Save(settings.Name, tenantConfig.ToObject<Dictionary<string, string>>());
        }
    }
}