using System;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateListFromTemplate : Command
    {
        public CreateListFromTemplate(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, ListTemplateType type, string title, Action<List> setAdditionalProperties = null)
        {
            Logger.Verbose($"Started executing {nameof(CreateListFromTemplate)} for title '{title}'", title);

            var web = ctx.Web;
            ctx.Load(web, w => w.Lists);
            ctx.ExecuteQuery();

            var candidate = web.Lists.SingleOrDefault(l => l.Title == title);
            if (candidate != null)
            {
                Logger.Warning($"List with title '{title}' already exist on web '{ctx.Url}'");
                return;
            }

            var newList =
                web.Lists.Add(
                    new ListCreationInformation
                    {
                        Title = title,
                        TemplateType = (int)type
                    });

            if (setAdditionalProperties != null)
            {
                setAdditionalProperties(newList);
                newList.Update();
            }

            ctx.ExecuteQuery();
        }
    }
}
