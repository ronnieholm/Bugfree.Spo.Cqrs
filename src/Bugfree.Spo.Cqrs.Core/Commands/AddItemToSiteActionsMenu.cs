using System;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddItemToSiteActionsMenu : Command
    {
        public AddItemToSiteActionsMenu(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string title, Uri url, BasePermissions visibleTo)
        {
            Logger.Verbose($"Started executing {nameof(AddItemToSiteActionsMenu)} for item '{title}'");

            // inspired by https://github.com/OfficeDev/PnP/tree/master/Scenarios/Provisioning.SiteModifier
            var web = ctx.Web;
            ctx.Load(web, w => w.UserCustomActions);
            ctx.ExecuteQuery();

            var existingItem = web.UserCustomActions.SingleOrDefault(a => a.Title == title);
            if (existingItem != null)
            {
                Logger.Warning($"Menu item '{title}' already exists");
                return;
            }

            var uca = web.UserCustomActions.Add();
            uca.Location = "Microsoft.SharePoint.StandardMenu";
            uca.Group = "SiteActions";
            uca.Rights = visibleTo;
            uca.Sequence = 100;
            uca.Title = title;
            uca.Url = url.ToString();
            uca.Update();
            ctx.ExecuteQuery();
        }
    }
}
