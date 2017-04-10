using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class RemoveGroupRoleAssignmentFromLibrary : Command
    {
        public RemoveGroupRoleAssignmentFromLibrary(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string groupName, List library)
        {
            Logger.Verbose($"About to execute {nameof(RemoveGroupRoleAssignmentFromLibrary)} for group '{groupName}' on library '{library.Title}'");

            var group = ctx.Web.SiteGroups.GetByName(groupName);
            ctx.Load(group);
            ctx.ExecuteQuery();

            var roleAssignment = library.RoleAssignments.SingleOrDefault(ra => ra.PrincipalId == group.Id);
            if (roleAssignment == null)
            {
                Logger.Warning($"Group '{groupName}' not associated with library '{library.Title}'");
                return;
            }

            library.RoleAssignments.GetByPrincipal(group).DeleteObject();
            library.Update();
            ctx.ExecuteQuery();
        }
    }
}
