using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentPreview.Models;
using OrchardCore.ContentPreview.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.ContentPreview.Settings
{
    public class PreviewPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly ILiquidTemplateManager _templateManager;

        public PreviewPartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<PreviewPartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            T = localizer;
        }

        public IStringLocalizer T { get; private set; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(PreviewPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<PreviewPartSettingsViewModel>("PreviewPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<PreviewPartSettings>();

                model.Pattern = settings.Pattern;
                model.PreviewPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(PreviewPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new PreviewPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, 
                m => m.Pattern
                );

            if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.Pattern), T["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            } else {
                context.Builder.WithSettings(new PreviewPartSettings
                {
                    Pattern = model.Pattern
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
