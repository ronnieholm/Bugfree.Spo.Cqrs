using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using System;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateSiteCollection : Command
    {
        public CreateSiteCollection(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, Uri url, string template, string owner, string title, int sizeInMb, uint languageId)
        {
            Logger.Verbose($"Started executing {nameof(CreateSiteCollection)} for url '{url}'");

            var tenant = new Tenant(ctx);
            ctx.Load(tenant);

            var siteCollections = tenant.GetSiteProperties(0, true);
            ctx.Load(siteCollections);
            ctx.ExecuteQuery();
            var siteCollection = siteCollections.SingleOrDefault(sc => sc.Url == url.ToString());

            if (siteCollection != null)
            {
                Logger.Warning($"Site collection at url '{url}' already exist");
                return;
            }

            var operation = tenant.CreateSite(new SiteCreationProperties
            {
                Url = url.ToString(),
                Owner = owner,
                Template = template,
                StorageMaximumLevel = sizeInMb,
                UserCodeMaximumLevel = 0,
                UserCodeWarningLevel = 0,
                // level is in absolute MBs, not percent
                StorageWarningLevel = (long)(sizeInMb * 0.9),
                Title = title,
                CompatibilityLevel = 15,
                TimeZoneId = 3,
                Lcid = languageId
            });

            ctx.Load(operation);
            ctx.ExecuteQuery();

            while (!operation.IsComplete)
            {
                System.Threading.Thread.Sleep(15000);
                ctx.Load(operation);
                ctx.ExecuteQuery();
                var status = operation.IsComplete ? "complete" : "waiting";
                Logger.Verbose($"Site creation status: {status}");
            }
        }
    }
}
