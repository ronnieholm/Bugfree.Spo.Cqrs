using System.IO;
using System.Reflection;

namespace Bugfree.Spo.Cqrs.Core.Utilities
{
    // todo: remove? Not used

    public class ResourceReader
    {
        readonly Assembly _current = Assembly.GetExecutingAssembly();

        public byte[] GetBinary(string id)
        {
            using (var s = _current.GetManifestResourceStream($"Bugfree.Spo.Cqrs.Core.Resources.{id}"))
            {
                var bytes = new byte[s.Length];
                s.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        public string GetString(string id)
        {
            using (var s = _current.GetManifestResourceStream($"Bugfree.Spo.Cqrs.Core.Resources.{id}"))
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
