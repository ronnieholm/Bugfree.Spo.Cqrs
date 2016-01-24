using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddGroupRoleAssignmentToLibrary : Command
    {
        public AddGroupRoleAssignmentToLibrary(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string groupName, IEnumerable<string> roleDefinitionNames, List library)
        {
            Logger.Verbose($"About to execute {nameof(AddGroupRoleAssignmentToLibrary)} for group '{groupName}' and library '{library.Title}'");

            var group = ctx.Web.SiteGroups.GetByName(groupName);
            var roleDefinitionBindingCollection = new RoleDefinitionBindingCollection(ctx);

            roleDefinitionNames.ToList().ForEach(name =>
            {
                var rd = ctx.Web.RoleDefinitions.GetByName(name);
                roleDefinitionBindingCollection.Add(rd);
            });

            library.RoleAssignments.Add(group, roleDefinitionBindingCollection);
            ctx.ExecuteQuery();
        }
    }
}
