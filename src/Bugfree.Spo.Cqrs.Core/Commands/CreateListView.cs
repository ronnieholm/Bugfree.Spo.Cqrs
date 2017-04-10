using System;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateListView : Command
    {
        public CreateListView(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string listTitle, string viewTitle, string[] fields, Action<View> setAdditionalProperties = null)
        {
            Logger.Verbose($"Started executing {nameof(CreateListView)} for view '{viewTitle}' on list '{listTitle}'");

            ctx.Load(ctx.Web.Lists);
            ctx.ExecuteQuery();

            var l = ctx.Web.Lists.GetByTitle(listTitle);
            ctx.Load(l, x => x.Views);
            ctx.ExecuteQuery();

            var view = l.Views.SingleOrDefault(v => v.Title == viewTitle);
            if (view != null)
            {
                Logger.Warning($"View '{viewTitle}' already exist on list {listTitle}");
                return;
            }

            var newView = l.Views.Add(new ViewCreationInformation
            {
                Title = viewTitle,
                ViewFields = fields
            });

            setAdditionalProperties?.Invoke(newView);
            newView.Update();
            ctx.ExecuteQuery();
        }
    }
}
