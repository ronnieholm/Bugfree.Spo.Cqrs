using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateListColumn : Command
    {
        public CreateListColumn(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string listTitle, XElement fieldXml, Action<Field> setAdditionalProperties = null)
        {
            var displayName = fieldXml.Attribute("DisplayName").Value;
            Logger.Verbose($"Started executing {nameof(CreateListColumn)} for column '{displayName}' on list '{listTitle}'");

            var list = ctx.Web.Lists.GetByTitle(listTitle);
            var fields = list.Fields;
            ctx.Load(fields);
            ctx.ExecuteQuery();

            var candidate = fields.SingleOrDefault(f => f.Title == displayName);
            if (candidate != null)
            {
                Logger.Warning($"Column '{displayName}' already on list '{listTitle}'");
                return;
            }

            var newField = fields.AddFieldAsXml(fieldXml.ToString(), true, AddFieldOptions.DefaultValue);
            ctx.Load(newField);
            ctx.ExecuteQuery();

            if (setAdditionalProperties != null)
            {
                setAdditionalProperties(newField);
                newField.Update();
            }
        }
    }
}
