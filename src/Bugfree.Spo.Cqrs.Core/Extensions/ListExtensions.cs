using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Extensions
{
    // deliberately not created as extension methods on ListCollection because of different semantics
    // compared to GetById and GetByTitle
    public class ListExtensions
    {
        public List GetListByUrl(ClientContext ctx, string url)
        {
            var lists = ctx.Web.Lists;
            ctx.Load(lists, l => l.Include(l2 => l2.DefaultViewUrl));
            ctx.ExecuteQuery();

            // take into account the different patterns of DefaultViewUrl:
            // document library: /sites/<siteCollection>/<web>/web>/<documentLibrary>/Forms/AllItems.aspx
            // list: /sites/<siteCollection>/<web>/<web>/Lists/<library>/AllItems.aspx
            return lists.ToList().SingleOrDefault(l =>
            {
                var tokens = l.DefaultViewUrl.Split(new[] { '/' });
                var isDocumentLibrary = tokens[tokens.Length - 2] == "Forms";
                var urlPart = isDocumentLibrary ? tokens[tokens.Length - 3] : tokens[tokens.Length - 2];
                return urlPart == url;
            });
        }
    }
}
