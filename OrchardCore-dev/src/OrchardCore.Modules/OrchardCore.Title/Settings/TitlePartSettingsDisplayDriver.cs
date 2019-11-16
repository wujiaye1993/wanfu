using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Title.Models;
using OrchardCore.Title.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.Title.Settings
{
    public class TitlePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ILiquidTemplateManager _templateManager;

        public TitlePartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<TitlePartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            T = localizer;
        }

        public IStringLocalizer T { get; private set; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(TitlePart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<TitlePartSettingsViewModel>("TitlePartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<TitlePartSettings>();

                model.Options = settings.Options;
                model.Pattern = settings.Pattern;
                model.TitlePartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(TitlePart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new TitlePartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, 
                m => m.Pattern,
                m => m.Options);

            if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.Pattern), T["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
            else
            {
                context.Builder.WithSettings(new TitlePartSettings {Pattern = model.Pattern, Options = model.Options,});
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}