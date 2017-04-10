using System;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class SetWelcomePage : Command
    {
        public SetWelcomePage(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string libraryName, string filePath)
        {
            Logger.Verbose($"Started executing {nameof(SetWelcomePage)} to '{filePath}' for library '{libraryName}'");

            var web = ctx.Web;
            var library = web.Lists.GetByTitle(libraryName);
            ctx.Load(library, l => l.RootFolder.ServerRelativeUrl, l => l.EntityTypeName);
            ctx.ExecuteQuery();

            var file = web.GetFileByServerRelativeUrl(library.RootFolder.ServerRelativeUrl + "/" + filePath);
            ctx.Load(file, f => f.Exists);
            ctx.ExecuteQuery();

            if (!file.Exists)
            {
                throw new InvalidOperationException($"File '{filePath}' not found in library '{library}'");
            }

            var rootFolder = web.RootFolder;
            ctx.Load(rootFolder);
            ctx.ExecuteQuery();

            var newWelcomePage = $"{library.EntityTypeName}/{filePath}";
            if (rootFolder.WelcomePage == newWelcomePage)
            {
                Logger.Warning("Welcome page has already been set");
                return;
            }

            rootFolder.WelcomePage = newWelcomePage;
            rootFolder.Update();
            ctx.ExecuteQuery();
        }
    }
}