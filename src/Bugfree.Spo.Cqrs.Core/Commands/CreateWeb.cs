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
            var webUrl = $"{ctx.Url}/{url}";
            Logger.Verbose($"Started executing {nameof(CreateWeb)} for url '{webUrl}'");                       
            ctx.Load(ctx.Web.Webs);
            ctx.ExecuteQuery();

            // Urls should normally be case-sensitive, but the CSOM API has been observed
            // to change the casing of part part of the URL across repeated calls to this 
            // command (reloads of webs collection). During the initial call it's "tests", 
            // during the second call it's becomes "Tests":
            //
            // https://bugfree.sharepoint.com/teams/Testbed/Tests/aff750d7-adac-4ceb-8dba-f54b313713ce/0f1af573-8bca-442a-9c82-f4ee35f6fb22
            //                                              ^
            //
            // This causes the command to wrongly assume the web doesn't exist and attempt
            // to recreate it, causing an exception to get thrown because the web address 
            // is already in use. Last verified on March 23, 2016 with CSOM NuGet version 
            // 16.1.5026.1200.
            var candidate = ctx.Web.Webs.SingleOrDefault(w => w.Url.ToLower() == webUrl.ToLower());
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
