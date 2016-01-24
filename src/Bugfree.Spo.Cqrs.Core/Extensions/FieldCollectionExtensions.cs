using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Extensions
{
    public class FieldCollectionExtensions
    {
        public T GetByInternalNameOrTitle<T>(FieldCollection fc, string nameOrTitle) where T : ClientObject
        {
            return fc.Context.CastTo<T>(fc.GetByInternalNameOrTitle(nameOrTitle));
        }
    }
}
