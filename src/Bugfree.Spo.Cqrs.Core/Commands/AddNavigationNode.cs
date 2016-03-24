using Microsoft.SharePoint.Client;
using System;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddNavigationNode : Command
    {
        public AddNavigationNode(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, NavigationNodeCollection nodes, string title, Uri target)
        {
            Logger.Verbose($"Started executing {nameof(AddNavigationNode)} for title '{title}' with url '{target}'");

            var candidate = nodes.SingleOrDefault(n => n.Title == title);
            if (candidate != null)
            {
                Logger.Warning($"Title '{title}' already on navigation");
                return;
            }

            nodes.Add(new NavigationNodeCreationInformation
            {
                Title = title,
                Url = target.ToString(),
                AsLastNode = true
            });

            ctx.ExecuteQuery();
        }
    }
}
