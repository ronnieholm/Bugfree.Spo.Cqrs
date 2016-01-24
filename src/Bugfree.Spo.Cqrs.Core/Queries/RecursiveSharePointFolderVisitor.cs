using Microsoft.SharePoint.Client;
using System;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Queries
{
    public class RecursiveSharePointFolderVisitor : Query
    {
        public RecursiveSharePointFolderVisitor(ILogger l) : base(l) { }

        private void RecursiveSharePointFolderVisitorInternal(Folder folder, Action<File> visit)
        {
            folder.Context.Load(folder, f => f.Files, f => f.Folders);
            folder.Context.ExecuteQuery();
            folder.Files.ToList().ForEach(f => visit(f));
            folder.Folders.ToList().ForEach(f => Execute(f, visit));
        }

        public void Execute(Folder folder, Action<File> visit)
        {
            var ctx = folder.Context;
            ctx.Load(folder, f => f.ServerRelativeUrl);
            ctx.ExecuteQuery();

            Logger.Verbose($"About to execute {nameof(RecursiveSharePointFolderVisitor)} for url: {folder.ServerRelativeUrl}");
            RecursiveSharePointFolderVisitorInternal(folder, visit);
        }
    }
}
