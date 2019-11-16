using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;
using OrchardCore.Media.ViewModels;
using OrchardCore.Media.Services;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Media.Drivers
{
    public class MediaFieldDisplayDriver : ContentFieldDisplayDriver<MediaField>
    {
        private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;

        public MediaFieldDisplayDriver(AttachedMediaFieldFileService attachedMediaFieldFileService,
            IStringLocalizer<MediaFieldDisplayDriver> localizer)
        {
            _attachedMediaFieldFileService = attachedMediaFieldFileService;
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public override IDisplayResult Display(MediaField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayMediaFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(MediaField field, BuildFieldEditorContext context)
        {
            var itemPaths = field.Paths?.ToList().Select(p => new EditMediaFieldItemInfo { Path = p }) ?? new EditMediaFieldItemInfo[] { };

            return Initialize<EditMediaFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Paths = JsonConvert.SerializeObject(itemPaths);
                model.TempUploadFolder = _attachedMediaFieldFileService.MediaFieldsTempSubFolder;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MediaField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditMediaFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, f => f.Paths))
            {
                // Deserializing an empty string doesn't return an array
                var items = string.IsNullOrWhiteSpace(model.Paths)
                    ? new List<EditMediaFieldItemInfo>()
                    : JsonConvert.DeserializeObject<EditMediaFieldItemInfo[]>(model.Paths).ToList();


                // If it's an attached media field editor the files are automatically handled by _attachedMediaFieldFileService
                if (string.Equals(context.PartFieldDefinition.Editor(), "Attached", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        await _attachedMediaFieldFileService.HandleFilesOnFieldUpdateAsync(items, context.ContentPart.ContentItem);
                    }
                    catch (Exception)
                    {
                        updater.ModelState.AddModelError(Prefix, S["{0}: There was an error handling the files.", context.PartFieldDefinition.DisplayName()]);
                    }
                }

                field.Paths = items.Where(p => !p.IsRemoved).Select(p => p.Path).ToArray() ?? new string[] {};


                var settings = context.PartFieldDefinition.GetSettings<MediaFieldSettings>();
                
                if (settings.Required && field.Paths.Length < 1)
                {
                    updater.ModelState.AddModelError(Prefix, S["{0}: A media is required.", context.PartFieldDefinition.DisplayName()]);
                }

                if (field.Paths.Length > 1 && !settings.Multiple)
                {
                    updater.ModelState.AddModelError(Prefix, S["{0}: Selecting multiple media is forbidden.", context.PartFieldDefinition.DisplayName()]);
                }                
            }

            return Edit(field, context);
        }
    }
}
