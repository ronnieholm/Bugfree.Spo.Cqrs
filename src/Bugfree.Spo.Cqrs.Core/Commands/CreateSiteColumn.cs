using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateSiteColumn : Command
    {
        public CreateSiteColumn(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, XElement fieldXml, Action<Field> setAdditionalProperties = null)
        {
            var internalName = fieldXml.Attribute("Name").Value;
            var group = fieldXml.Attribute("Group").Value;
            Logger.Verbose($"Started executing {nameof(CreateSiteColumn)} for column '{internalName}' in group '{group}'");

            var fields = ctx.Web.Fields;
            ctx.Load(fields);
            ctx.ExecuteQuery();

            var field = fields.SingleOrDefault(f => f.InternalName == internalName && f.Group == group);
            if (field != null)
            {
                Logger.Warning($"Column '{internalName}' in group '{group}' already exist on '{ctx.Url}");
                return;
            }

            var newField = fields.AddFieldAsXml(fieldXml.ToString(), false, AddFieldOptions.AddFieldInternalNameHint);
            ctx.Load(newField);
            ctx.ExecuteQuery();

            if (setAdditionalProperties != null)
            {
                setAdditionalProperties(newField);
                newField.Update();
                ctx.ExecuteQuery();
            }
        }
    }
}
