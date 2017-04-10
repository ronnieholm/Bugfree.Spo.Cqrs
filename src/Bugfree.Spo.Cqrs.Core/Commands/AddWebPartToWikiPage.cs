using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using E = System.Xml.Linq.XElement;
using A = System.Xml.Linq.XAttribute;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddWebPartToWikiPage : Command
    {
        public AddWebPartToWikiPage(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string libraryName, string filePath, E webPart, int row, int column)
        {
            XNamespace v2 = "http://schemas.microsoft.com/WebPart/v2";
            XNamespace v3 = "http://schemas.microsoft.com/WebPart/v3";
            string title = null;

            var v3Prope = webPart.Elements().ToList().SingleOrDefault(e => e.Name.LocalName == "webPart");
            if (webPart.Name.Namespace == v2)
            {
                title = webPart.Element(v2 + "Title").Value;
            }
            else if (v3Prope != null && v3Prope.Name.Namespace == v3)
            {
                var properties = v3Prope.Element(v3 + "data").Element(v3 + "properties").Elements(v3 + "property");
                title = properties.Single(e => e.Attribute("name").Value == "Title").Value;
            }
            else 
            {
                throw new ArgumentException("Unable to extract title for webpart definition");
            }
            Logger.Verbose($"Started executing {nameof(AddWebPartToWikiPage)} with title '{title}' to page '{filePath}'");

            var web = ctx.Web;
            var library = web.Lists.GetByTitle(libraryName);
            ctx.Load(library, l => l.RootFolder);
            ctx.ExecuteQuery();

            var page = web.GetFileByServerRelativeUrl(library.RootFolder.ServerRelativeUrl + "/" + filePath);
            ctx.Load(library, l => l.RootFolder);
            ctx.Load(page, p => p.Exists);
            ctx.ExecuteQuery();

            if (!page.Exists)
            {
                throw new ArgumentException($"File path '{filePath}' not found in library '{libraryName}'");
            }

            var fields = page.ListItemAllFields;
            ctx.Load(page, p => p.ListItemAllFields);
            ctx.ExecuteQuery();

            var wikiField = E.Parse((string)fields["WikiField"]);
            var rows = wikiField.Element("table").Element("tbody").Elements("tr");
            if (row >= rows.Count())
            {
                throw new ArgumentException($"Cannot insert into row {row}. Page only contains {rows.Count()} rows");
            }

            var columns = rows.ElementAt(row).Elements("td");
            if (column >= columns.Count())
            {
                throw new ArgumentException($"Cannot intert into column {column}. Page only contains {columns.Count()} columns");
            }

            var manager = page.GetLimitedWebPartManager(PersonalizationScope.Shared);
            ctx.Load(manager, m => m.WebParts.Include(m2 => m2.WebPart.Title));
            ctx.ExecuteQuery();

            var candidate = manager.WebParts.SingleOrDefault(wpd => wpd.WebPart.Title == title);
            if (candidate != null)
            {
                Logger.Warning($"Web part with title '{title}' already on page");
                return;
            }

            var definition = manager.ImportWebPart(webPart.ToString());
            var newWebPart = manager.AddWebPart(definition.WebPart, "wpz", 0);
            ctx.Load(newWebPart);
            ctx.ExecuteQuery();

            var id = newWebPart.Id;
            var layoutsZoneInner = columns.ElementAt(column);
            layoutsZoneInner
                .Element("div")
                .Element("div")
                .Add(new E("p", ""),
                    new E("div", new A("class", "ms-rtestate-read ms-rte-wpbox"),
                        new E("div", new A("class", "ms-rtestate-notify ms-rtestate-read " + id), new A("id", "div_" + id), new A("unselectable", "on"), ""),
                        new E("div", new A("id", "vid_" + id), new A("unselectable", "on"), new A("style", "display:none;"), "")));

            fields["WikiField"] = wikiField.ToString();
            fields.Update();
            ctx.ExecuteQuery();
        }
    }
}
