using System;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateContentType : Command
    {
        public CreateContentType(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string contentTypeName, string contentTypeGroup, string parentContenttypeName, Action<ContentType> setAdditionalProperties = null)
        {
            Logger.Verbose($"Started executing {nameof(CreateContentType)} for name '{contentTypeName}'");

            var web = ctx.Site.RootWeb;
            var contentTypes = web.ContentTypes;
            ctx.Load(contentTypes);
            ctx.ExecuteQuery();

            var candidate = contentTypes.SingleOrDefault(ct => ct.Name == contentTypeName);
            if (candidate != null)
            {
                Logger.Warning($"Content type with name '{contentTypeName}' already exists on web '{ctx.Url}'");
                return;
            }

            var parent = contentTypes.SingleOrDefault(ct => ct.Name == parentContenttypeName);
            if (parent == null)
            {
                Logger.Error($"Parent content type with name '{contentTypeName}' not found on web '{ctx.Url}'");
                return;
            }

            var newContentType =
                web.ContentTypes.Add(
                    new ContentTypeCreationInformation
                    {
                        Name = contentTypeName,
                        Group = contentTypeGroup,
                        ParentContentType = parent
                    });
            ctx.ExecuteQuery();

            if (setAdditionalProperties != null)
            {
                setAdditionalProperties(newContentType);
                newContentType.Update(false);
            }
        }
    }
}
