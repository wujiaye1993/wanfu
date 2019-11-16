using System.Threading.Tasks;
using Fluid;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Html.GraphQL
{
    public class HtmlBodyQueryObjectType : ObjectGraphType<HtmlBodyPart>
    {
        public HtmlBodyQueryObjectType(IStringLocalizer<HtmlBodyQueryObjectType> T)
        {
            Name = "HtmlBodyPart";
            Description = T["Content stored as HTML."];

            Field<StringGraphType>()
                .Name("html")
                .Description(T["the HTML content"])
                .ResolveAsync(RenderHtml);
        }

        private static async Task<object> RenderHtml(ResolveFieldContext<HtmlBodyPart> ctx)
        {
            var context = (GraphQLContext) ctx.UserContext;
            var liquidTemplateManager = context.ServiceProvider.GetService<ILiquidTemplateManager>();

            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", ctx.Source.ContentItem);
            templateContext.MemberAccessStrategy.Register<HtmlBodyPartViewModel>();

            return await liquidTemplateManager.RenderAsync(ctx.Source.Html, templateContext);
        }
    }
}
