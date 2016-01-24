using Microsoft.SharePoint.Client;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class RemoveListView : Command
    {
        public RemoveListView(ILogger l) : base(l) { }

        // beware that if you delete the last view on a list which was previously
        // added to quick launch, SharePoint will, on its own accord, remove the 
        // list from quick launch. Even though the action is unrelated to added.
        public void Execute(ClientContext ctx, string listTitle, string viewTitle)
        {
            Logger.Verbose($"Started executing {nameof(RemoveListView)} for view '{viewTitle}' on list url '{listTitle}'");

            var list = ctx.Web.Lists.GetByTitle(listTitle);
            ctx.Load(list, l => l.Views, l => l.SchemaXml);
            ctx.ExecuteQuery();

            var existingView = list.Views.SingleOrDefault(v => v.Title == viewTitle);
            if (existingView == null)
            {
                Logger.Warning($"View '{viewTitle}' not found on list '{listTitle}'");
                return;
            }

            existingView.DeleteObject();
            ctx.ExecuteQuery();
        }
    }
}
