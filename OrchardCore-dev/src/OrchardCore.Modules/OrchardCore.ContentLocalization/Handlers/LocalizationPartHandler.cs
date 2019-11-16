using System;
using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Localization;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class LocalizationPartHandler : ContentPartHandler<LocalizationPart>
    {
        private readonly ILocalizationEntries _entries;

        public LocalizationPartHandler(ILocalizationEntries entries)
        {
            _entries = entries;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LocalizationPart part)
        {
            return context.ForAsync<CultureAspect>(cultureAspect =>
            {
                cultureAspect.Culture = CultureInfo.GetCultureInfo(part.Culture);
                return Task.CompletedTask;
            });
        }

        public override Task PublishedAsync(PublishContentContext context, LocalizationPart part)
        {
            if (!String.IsNullOrWhiteSpace(part.LocalizationSet))
            {
                _entries.AddEntry(new LocalizationEntry()
                {
                    ContentItemId = part.ContentItem.ContentItemId,
                    LocalizationSet = part.LocalizationSet,
                    Culture = part.Culture.ToLowerInvariant()
                });
            }

            return Task.CompletedTask;
        }

        public override Task UnpublishedAsync(PublishContentContext context, LocalizationPart part)
        {
            _entries.RemoveEntry(new LocalizationEntry()
            {
                ContentItemId = part.ContentItem.ContentItemId,
                LocalizationSet = part.LocalizationSet,
                Culture = part.Culture.ToLowerInvariant()
            });

            return Task.CompletedTask;
        }

        public override Task RemovedAsync(RemoveContentContext context, LocalizationPart part)
        {
            _entries.RemoveEntry(new LocalizationEntry()
            {
                ContentItemId = part.ContentItem.ContentItemId,
                LocalizationSet = part.LocalizationSet,
                Culture = part.Culture.ToLowerInvariant()
            });

            return Task.CompletedTask;
        }
    }
}
