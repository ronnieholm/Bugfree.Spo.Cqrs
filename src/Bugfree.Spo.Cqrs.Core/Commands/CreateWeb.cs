using Microsoft.SharePoint.Client;
using System;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateWeb : Command
    {
        public CreateWeb(ILogger l) : base(l) { }

        public void Execute(
            ClientContext ctx, string title, string url, string template,
            bool useSamePermissionsAsParentSite, int languageId,
            Action<Web> setAdditionalProperties = null)
        {
            Logger.Verbose($"Started executing {nameof(CreateWeb)} for url '{url}'");

            var webUrl = ctx.Url + "/" + url;
            ctx.Load(ctx.Web.Webs);
            ctx.ExecuteQuery();

            var candidate = ctx.Web.Webs.SingleOrDefault(w => w.Url == webUrl);
            if (candidate != null)
            {
                Logger.Warning($"Web with url '{webUrl}' already exists");
                return;
            }

            var newWeb =
                ctx.Web.Webs.Add(
                    new WebCreationInformation
                    {
                        WebTemplate = template,
                        Title = title,
                        Url = url,
                        Language = languageId,
                        UseSamePermissionsAsParentSite = useSamePermissionsAsParentSite
                    });
            ctx.ExecuteQuery();

            if (setAdditionalProperties != null)
            {
                ctx.Load(newWeb);
                ctx.ExecuteQuery();
                setAdditionalProperties(newWeb);
                newWeb.Update();
                ctx.ExecuteQuery();
            }
        }
    }
}
