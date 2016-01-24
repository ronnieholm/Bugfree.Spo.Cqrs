using Microsoft.SharePoint.Client;
using System;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddLinkToLinkList : Command
    {
        public AddLinkToLinkList(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string library, Uri url, string description)
        {
            Logger.Verbose($"Started executing {nameof(AddLinkToLinkList)} for url '{url}' on library '{library}'");

            var web = ctx.Web;
            var links = web.Lists.GetByTitle(library);
            var result = links.GetItems(CamlQuery.CreateAllItemsQuery());

            ctx.Load(result);
            ctx.ExecuteQuery();

            var existingLink =
                result
                    .ToList()
                    .Any(l =>
                    {
                        var u = (FieldUrlValue)l.FieldValues["URL"];
                        return u.Url == url.ToString() && u.Description == description;
                    });

            if (existingLink)
            {
                Logger.Warning($"Link '{url}' with description '{description}' already exists");
                return;
            }

            var newLink = links.AddItem(new ListItemCreationInformation());
            newLink["URL"] = new FieldUrlValue { Url = url.ToString(), Description = description };
            newLink.Update();
            ctx.ExecuteQuery();
        }
    }
}
