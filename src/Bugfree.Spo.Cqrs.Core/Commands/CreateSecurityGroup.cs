using System;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateSecurityGroup : Command
    {
        public CreateSecurityGroup(ILogger l) : base(l) { }

        // when SharePoint creates security groups for owners, visitors, and
        // members upon web creation, these groups aren't created at web-level. 
        // Instead they're created at site level and referenced from web level.
        // Thus, running group creation code multiple times, even after removing 
        // the group from a web, it may fail because the group will still be 
        // present at the site level.
        public void Execute(ClientContext ctx, string groupTitle, string groupDescription, Action<Group> setAdditionalProperties = null)
        {
            Logger.Verbose($"Started executing {nameof(CreateSecurityGroup)} for group with title '{groupTitle}'");
            var groups = ctx.Site.RootWeb.SiteGroups;
            ctx.Load(groups);
            ctx.ExecuteQuery();

            var existingGroup = groups.SingleOrDefault(g => g.Title == groupTitle);
            if (existingGroup != null)
            {
                Logger.Warning($"Group with title '{groupTitle}' already exist in site collection");
                return;
            }

            var newGroup = groups.Add(
                new GroupCreationInformation
                {
                    Title = groupTitle,
                    Description = groupDescription
                });

            ctx.ExecuteQuery();

            if (setAdditionalProperties != null)
            {
                setAdditionalProperties(newGroup);
            }
            newGroup.Update();
            ctx.ExecuteQuery();
        }
    }
}
