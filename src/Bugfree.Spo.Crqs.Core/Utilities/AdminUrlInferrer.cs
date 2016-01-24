using System;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Utilities
{
    public class AdminUrlInferrer
    {
        public Uri InferAdminFromTenant(Uri tenant)
        {
            const char seperator = '.';
            return new Uri(
                tenant
                .ToString()
                .Split(seperator)
                .Aggregate("", (acc, c) =>
                    acc == ""
                        ? c + "-admin"
                        : acc + seperator + c));
        }
    }
}
