using System;
using System.IO;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Queries
{
    public class RecursiveNativeFolderVisitor : Query
    {
        public RecursiveNativeFolderVisitor(ILogger l) : base(l) { }

        public void Execute(string path, Action<FileSystemInfo> visit)
        {
            Logger.Verbose($"About to execute {nameof(RecursiveNativeFolderVisitor)} for path: {path}");
            new DirectoryInfo(path).GetFiles().ToList().ForEach(visit);
            new DirectoryInfo(path).GetDirectories().ToList().ForEach(f => Execute(f.FullName, visit));
        }
    }
}
