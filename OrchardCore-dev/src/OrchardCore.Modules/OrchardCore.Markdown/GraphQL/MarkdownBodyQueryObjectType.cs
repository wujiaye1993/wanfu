using System.Threading.Tasks;
using Fluid;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;

namespace OrchardCore.Markdown.GraphQL
{
    public class MarkdownBodyQueryObjectType : ObjectGraphType<MarkdownBodyPart>
    {
        public MarkdownBodyQueryObjectType(IStringLocalizer<MarkdownBodyQueryObjectType> T)
        {
            Name = nameof(MarkdownBodyPart);
            Description = T["Content stored as Markdown. You can also query the HTML interpreted version of Markdown."];

            Field("markdown", x => x.Markdown, nullable: true)
                .Description(T["the markdown value"])
                .Type(new StringGraphType());

            Field<StringGraphType>()
                .Name("html")
                .Description(T["the HTML representation of the markdown content"])
                .ResolveAsync(ToHtml);
        }

        private static async Task<object> ToHtml(ResolveFieldContext<MarkdownBodyPart> ctx)
        {
            var context = (GraphQLContext) ctx.UserContext;
            var liquidTemplateManager = context.ServiceProvider.GetService<ILiquidTemplateManager>();

            var markdown = ctx.Source.Markdown;
            var templateContext = new TemplateContext();
            markdown = await liquidTemplateManager.RenderAsync(markdown, templateContext);

            return Markdig.Markdown.ToHtml(markdown);
        }
    }
}
