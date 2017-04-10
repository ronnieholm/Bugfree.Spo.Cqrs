using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class RemoveContentTypeFromList : Command
    {
        public RemoveContentTypeFromList(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string libraryTitle, string contentTypeName)
        {
            Logger.Verbose($"Started executing {nameof(RemoveContentTypeFromList)} of content types '{contentTypeName}' from list '{libraryTitle}'");

            var library = ctx.Web.Lists.GetByTitle(libraryTitle);
            ctx.Load(library, l => l.ContentTypes);
            ctx.ExecuteQuery();

            var candidate = library.ContentTypes.FirstOrDefault(ct => ct.Name == contentTypeName);
            if (candidate == null)
            {
                Logger.Warning($"Content type '{contentTypeName}' not associated with library");
                return;
            }

            candidate.DeleteObject();
            ctx.ExecuteQuery();
        }
    }
}
