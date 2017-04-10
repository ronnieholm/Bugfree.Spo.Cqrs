using System;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddFieldToContentType : Command
    {
        public AddFieldToContentType(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string internalNameOrTitleOfField, string contentTypeName, Action<FieldLink> setAdditionalProperties = null)
        {
            Logger.Verbose($"Started executing {nameof(AddFieldToContentType)} for field '{internalNameOrTitleOfField}' on content type '{contentTypeName}'");

            var web = ctx.Site.RootWeb;
            var contentTypes = web.ContentTypes;
            var fields = web.Fields;

            ctx.Load(web);
            ctx.Load(contentTypes);
            ctx.Load(fields);
            ctx.ExecuteQuery();

            var contentType = contentTypes.Single(ct => ct.Name == contentTypeName);
            var fieldLinks = contentType.FieldLinks;
            ctx.Load(fieldLinks);
            ctx.ExecuteQuery();

            var candidate = fieldLinks.SingleOrDefault(fl => fl.Name == internalNameOrTitleOfField);
            if (candidate != null)
            {
                Logger.Warning($"Field '{internalNameOrTitleOfField}' already exists on content type '{contentTypeName}'");
                return;
            }

            var field = fields.GetByInternalNameOrTitle(internalNameOrTitleOfField);
            var newFieldLink = contentType.FieldLinks.Add(
                new FieldLinkCreationInformation
                {
                    Field = field
                });

            contentType.Update(true);
            ctx.ExecuteQuery();

            ctx.Load(newFieldLink);
            ctx.ExecuteQuery();

            if (setAdditionalProperties != null)
            {
                setAdditionalProperties(newFieldLink);
                contentType.Update(true);
                ctx.ExecuteQuery();
            }
        }
    }
}