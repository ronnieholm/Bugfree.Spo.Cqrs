using Bugfree.Spo.Cqrs.Core.Utilities;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Queries 
{
    public class GetTenantSiteCollections : Query 
    {
        public GetTenantSiteCollections(ILogger l) : base(l) { }

        private List<SiteProperties> GetTenantSiteCollectionsRecursive(Tenant tenant, List<SiteProperties> siteProperties, int startPosition) 
        {
            Logger.Verbose($"Fetching tenant site collections starting from position {startPosition}");
            var tenantSiteCollections = tenant.GetSiteProperties(startPosition, true);
            tenant.Context.Load(tenantSiteCollections);
            tenant.Context.ExecuteQuery();

            var newSiteProperties = siteProperties.Concat(tenantSiteCollections).ToList();

            return tenantSiteCollections.NextStartIndex == -1
                ? newSiteProperties
                : GetTenantSiteCollectionsRecursive(tenant, newSiteProperties, tenantSiteCollections.NextStartIndex);
        }

        public List<SiteProperties> Execute(ClientContext ctx) 
        {
            Logger.Verbose($"About to execute {nameof(GetTenantSiteCollections)}");
            var url = ctx.Url;
            var tenantAdminUrl = new AdminUrlInferrer().InferAdminFromTenant(new Uri(url.Replace(new Uri(url).AbsolutePath, "")));
            var tenantAdminCtx = new ClientContext(tenantAdminUrl) { Credentials = ctx.Credentials };
            var tenant = new Tenant(tenantAdminCtx);
            tenantAdminCtx.Load(tenant);
            tenantAdminCtx.ExecuteQuery();
            return GetTenantSiteCollectionsRecursive(tenant, new List<SiteProperties>(), 0);
        }
    }
}
