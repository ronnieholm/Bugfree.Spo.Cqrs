using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreatePermissionLevel : Command
    {
        public CreatePermissionLevel(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string permissionLevelName, string permissionLevelDescription, BasePermissions permissions)
        {
            Logger.Verbose($"About to execute {nameof(CreatePermissionLevel)} for level '{permissionLevelName}' on web '{ctx.Url}'");

            var roleDefinitions = ctx.Web.RoleDefinitions;
            ctx.Load(roleDefinitions);
            ctx.ExecuteQuery();

            var roleDefinition = roleDefinitions.SingleOrDefault(rd => rd.Name == permissionLevelName);
            if (roleDefinition != null)
            {
                Logger.Warning($"Permission level '{permissionLevelName}' already exists");
                return;
            }

            ctx.Web.RoleDefinitions.Add(
                new RoleDefinitionCreationInformation
                {
                    BasePermissions = permissions,
                    Name = permissionLevelName,
                    Description = permissionLevelDescription
                });
            ctx.ExecuteQuery();
        }
    }
}
