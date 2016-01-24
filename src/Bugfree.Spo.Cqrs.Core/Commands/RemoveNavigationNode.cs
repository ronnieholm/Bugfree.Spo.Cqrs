using Microsoft.SharePoint.Client;
using System;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class RemoveNavigationNode : Command
    {
        public enum Navigation
        {
            None = 0,
            QuickLaunch,
            TopNavigationBar
        }

        public RemoveNavigationNode(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, Navigation source, string title)
        {
            Logger.Verbose($"Started executing {nameof(RemoveNavigationNode)} for item '{title}' in '{source}' on url '{ctx.Url}'");

            var navigation = ctx.Web.Navigation;
            NavigationNodeCollection nodes = null;
            switch (source)
            {
                case Navigation.None:
                    throw new ArgumentException("Uninitialized navigation source");
                case Navigation.QuickLaunch:
                    nodes = navigation.QuickLaunch;
                    break;
                case Navigation.TopNavigationBar:
                    nodes = navigation.TopNavigationBar;
                    break;
                default:
                    throw new ArgumentException($"Unsupported navigation source: {source}");
            }

            ctx.Load(nodes);
            ctx.ExecuteQuery();

            var candidate = nodes.SingleOrDefault(n => n.Title == title);
            if (candidate == null)
            {
                Logger.Warning($"Navigation item '{title}' not present");
                return;
            }

            candidate.DeleteObject();
            ctx.ExecuteQuery();
        }
    }
}
