using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddWebPartToPage : Command
    {
        public AddWebPartToPage(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string libraryName, string filePath, XElement webpart, string zoneId, int zoneIndex)
        {
            var title = webpart.Elements().First(e => e.Name.LocalName == "Title").Value;
            Logger.Verbose($"Started executing {nameof(AddWebPartToPage)} with title '{title}' to file path '{filePath}'");

            var web = ctx.Web;
            var library = web.Lists.GetByTitle(libraryName);
            var page = web.GetFileByServerRelativeUrl(library.RootFolder.ServerRelativeUrl + "/" + filePath);
            ctx.Load(page, p => p.Exists);
            ctx.ExecuteQuery();

            if (!page.Exists)
            {
                throw new ArgumentException($"File path '{filePath}' not found in library '{libraryName}'");
            }

            var manager = page.GetLimitedWebPartManager(PersonalizationScope.Shared);
            ctx.Load(manager, m => m.WebParts);
            ctx.ExecuteQuery();

            foreach (var wp in manager.WebParts.Select(d => d.WebPart))
            {
                ctx.Load(wp);
                ctx.ExecuteQuery();

                if (wp.Title == title)
                {
                    Logger.Warning($"Web part with title '{title}' already present on page");
                    return;
                }
            }

            var definition = manager.ImportWebPart(webpart.ToString());
            manager.AddWebPart(definition.WebPart, zoneId, zoneIndex);
            ctx.ExecuteQuery();
        }
    }
}
