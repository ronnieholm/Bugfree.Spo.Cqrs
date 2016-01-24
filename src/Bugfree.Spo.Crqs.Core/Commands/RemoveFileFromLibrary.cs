using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class RemoveFileFromLibrary : Command
    {
        public RemoveFileFromLibrary(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string libraryName, string filePath)
        {
            Logger.Verbose($"Started executing {nameof(RemoveFileFromLibrary)} for file '{filePath}' in library '{libraryName}'");

            var web = ctx.Web;
            var library = web.Lists.GetByTitle(libraryName);
            ctx.Load(library, l => l.RootFolder.ServerRelativeUrl);
            ctx.ExecuteQuery();

            var file = web.GetFileByServerRelativeUrl(library.RootFolder.ServerRelativeUrl + "/" + filePath);
            ctx.Load(file, f => f.Exists);
            ctx.ExecuteQuery();

            if (!file.Exists)
            {
                Logger.Warning($"File '{filePath}' not found in library '{libraryName}'");
                return;
            }

            file.DeleteObject();
            ctx.ExecuteQuery();
        }
    }
}
