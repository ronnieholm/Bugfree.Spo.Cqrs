using Microsoft.SharePoint.Client;
using System;
using System.Linq;

// todo: add setAdditionalProperties that passes along the content type passed to list if you with
//       to override anything in that inherited one

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddContentTypeToList : Command
    {
        public AddContentTypeToList(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string libraryTitle, string contentTypeName)
        {
            Logger.Verbose($"Started executing {nameof(AddContentTypeToList)} for list '{libraryTitle}' and content type '{contentTypeName}'");

            var library = ctx.Web.Lists.GetByTitle(libraryTitle);
            var availableContentTypes = ctx.Site.RootWeb.ContentTypes;
            ctx.Load(library, l => l.ContentTypes);
            ctx.Load(availableContentTypes);
            ctx.ExecuteQuery();

            var candidate = library.ContentTypes.SingleOrDefault(ct => ct.Name == contentTypeName);
            if (candidate != null)
            {
                Logger.Warning($"Content type '{contentTypeName}' already added to list");
                return;
            }

            // todo: SingleOrDefault?
            var contentTypeToAdd = availableContentTypes.FirstOrDefault(ct => ct.Name == contentTypeName);
            if (contentTypeToAdd == null)
            {
                throw new ArgumentException($"Content type '{contentTypeName}' not found");
            }

            library.ContentTypes.AddExistingContentType(contentTypeToAdd);
            library.Update();
            ctx.ExecuteQuery();
        }
    }
}
