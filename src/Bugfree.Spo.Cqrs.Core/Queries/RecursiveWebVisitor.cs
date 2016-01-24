using Microsoft.SharePoint.Client;
using System;
using System.Linq;
using System.Net;

namespace Bugfree.Spo.Cqrs.Core.Queries
{
    public class RecursiveWebVisitor : Query
    {
        public RecursiveWebVisitor(ILogger l) : base(l) { }

        public void Execute(ICredentials c, Uri url, Action<Web> visit)
        {
            Logger.Verbose($"About to execute {nameof(RecursiveWebVisitor)} for '{url}'");

            using (var ctx = new ClientContext(url) { Credentials = c })
            {
                var webs = ctx.Web.Webs;
                ctx.Load(webs);
                ctx.ExecuteQuery();

                webs.ToList().ForEach(w =>
                {
                    visit(w);
                    Execute(c, new Uri(w.Url), visit);
                });
            }
        }
    }
}
