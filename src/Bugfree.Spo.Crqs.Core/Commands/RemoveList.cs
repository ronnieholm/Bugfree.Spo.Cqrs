using Microsoft.SharePoint.Client;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class RemoveList : Command
    {
        public RemoveList(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string listTitle)
        {
            Logger.Verbose($"Started executing {nameof(RemoveList)} for list '{listTitle}' on web '{ctx.Url}'");

            ctx.Load(ctx.Web, w => w.Lists);
            ctx.ExecuteQuery();

            var candidate = ctx.Web.Lists.SingleOrDefault(l => l.Title == listTitle);
            if (candidate == null)
            {
                Logger.Warning($"List '{listTitle}' not found on web");
                return;
            }

            candidate.DeleteObject();
            ctx.ExecuteQuery();
        }
    }
}
