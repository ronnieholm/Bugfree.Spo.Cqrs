using System;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class SetDefaultGroupAssociation : Command
    {
        public enum GroupAssociation
        {
            None = 0,
            OwnerGroup,
            MemberGroup,
            VisitorGroup
        };

        public SetDefaultGroupAssociation(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string groupTitle, GroupAssociation association)
        {
            Logger.Verbose($"Started executing {nameof(SetDefaultGroupAssociation)} for group '{groupTitle}' on web '{ctx.Url}'");

            var web = ctx.Web;
            var groups = web.SiteGroups;
            ctx.Load(groups);
            ctx.ExecuteQuery();

            var group = groups.SingleOrDefault(g => g.Title == groupTitle);
            if (group == null)
            {
                throw new InvalidOperationException($"Group '{groupTitle}' not found");
            }

            switch (association)
            {
                case GroupAssociation.None:
                    throw new InvalidOperationException("None is an invalid group association");
                case GroupAssociation.OwnerGroup:
                    web.AssociatedOwnerGroup = group;
                    break;
                case GroupAssociation.MemberGroup:
                    web.AssociatedMemberGroup = group;
                    break;
                case GroupAssociation.VisitorGroup:
                    web.AssociatedVisitorGroup = group;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported enum option: {association}");
            }
            web.Update();
            ctx.ExecuteQuery();
        }
    }
}
