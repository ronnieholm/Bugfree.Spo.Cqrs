using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateColumnOnList : Command
    {
        public CreateColumnOnList(ILogger l) : base(l) { }

        public void Execute(ClientContext ctx, string listTitle, XElement schema, Action<Field> setAdditionalProperties = null)
        {
            var displayName = schema.Attribute("DisplayName").Value;
            Logger.Verbose($"Started executing {nameof(CreateColumnOnList)} for column '{displayName}' on list '{listTitle}'");

            var list = ctx.Web.Lists.GetByTitle(listTitle);
            var fields = list.Fields;
            ctx.Load(fields);
            ctx.ExecuteQuery();

            // if using internal names in code, remember to encode those before querying, e.g., 
            // space character becomes "_x0020_" as described here:
            // http://www.n8d.at/blog/encode-and-decode-field-names-from-display-name-to-internal-name/
            // We don't use the internal name here because it's limited to 32 chacters. This means
            // "Secondary site abcde" is the longest possible internal name when taken into account
            // the "_x0020_" character. Using a longer name will truncate the internal name to 32
            // characters. Thus querying on the complete, not truncated name, will always return no
            // results which causes the field get created repetedly.
            var field = fields.SingleOrDefault(f => f.Title == displayName);
            if (field != null)
            {
                Logger.Warning($"Column '{displayName}' already on list {listTitle}");
                return;
            }

            var newField = list.Fields.AddFieldAsXml(schema.ToString(), true, AddFieldOptions.DefaultValue);
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
