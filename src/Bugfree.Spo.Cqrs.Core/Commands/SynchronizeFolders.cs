using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bugfree.Spo.Cqrs.Core.Queries;
using SPFile = Microsoft.SharePoint.Client.File;
using SPFolder = Microsoft.SharePoint.Client.Folder;

// todo: work in progress

/*
    Action<SynchronizeFolders.FileSystemOperation> alwaysOverride = o => {
        switch (o.Kind) {
            case SynchronizeFolders.OperationKind.None:
                throw new ArgumentException("Bug in FolderSynchronize command");
            case SynchronizeFolders.OperationKind.Copy:
                var content = FSFile.ReadAllBytes(o.Operand.SystemPath);
                var components = o.Operand.NormalizedPath.Split(new[] { '/' });
                var pathWithoutFile = components.Take(components.Length - 1).Aggregate("", (acc, c) => acc == "" ? c : acc + "/" + c);
                if (pathWithoutFile.Length > 0) {
                    new CreateFolderPath(logger).Execute(ctx, "Documents", pathWithoutFile);
                }
                new AddFileToLibrary(logger).Execute(ctx, content, "Documents", o.Operand.NormalizedPath);
                break;
            case SynchronizeFolders.OperationKind.Delete:
                new RemoveFileFromLibrary(logger).Execute(ctx, "Documents", o.Operand.NormalizedPath);
                break;
            default:
                throw new ArgumentException("Unsupported OperationKind: " + o.Kind);
        }
    };

    new SynchronizeFolders(logger).Execute(@"C:\Files", documents.RootFolder, alwaysOverride);
*/

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class SynchronizeFolders : Command
    {
        public struct FileSystemEntry
        {
            public string SystemPath { get; set; }
            public DateTime LastModifiedAt { get; set; }
            public string NormalizedPath { get; set; }
        }

        public enum OperationKind
        {
            None = 0,
            Copy,
            Delete
        }

        public struct FileSystemOperation
        {
            public OperationKind Kind { get; set; }
            public FileSystemEntry Operand { get; set; }
        }

        public SynchronizeFolders(ILogger l) : base(l) { }

        public void Execute(string absoluteNativePath, SPFolder folder, Action<FileSystemOperation> operation)
        {
            var ctx = folder.Context;
            ctx.Load(folder, f => f.ServerRelativeUrl);
            ctx.ExecuteQuery();

            var nativeFileVisitor = new List<FileSystemEntry>();
            Action<FileSystemInfo> visitor = f =>
                nativeFileVisitor.Add(new FileSystemEntry
                {
                    SystemPath = f.FullName,
                    LastModifiedAt = f.LastWriteTimeUtc,
                    NormalizedPath = f.FullName.Replace(absoluteNativePath + "\\", "").Replace("\\", "/"),
                });

            var sharePointFilesVisitor = new List<FileSystemEntry>();
            Action<SPFile> sharePointFileVisitor = f =>
            {
                var normalizedPath = f.ServerRelativeUrl.Replace(folder.ServerRelativeUrl + "/", "");
                if (normalizedPath.StartsWith("Forms"))
                {
                    return;
                }

                sharePointFilesVisitor.Add(new FileSystemEntry
                {
                    SystemPath = f.ServerRelativeUrl,
                    LastModifiedAt = f.TimeLastModified,
                    NormalizedPath = normalizedPath
                });
            };

            new RecursiveNativeFolderVisitor(Logger).Execute(absoluteNativePath, visitor);
            new RecursiveSharePointFolderVisitor(Logger).Execute(folder, sharePointFileVisitor);

            var toDelete = sharePointFilesVisitor.Except(nativeFileVisitor);
            var toCopy = nativeFileVisitor.Except(sharePointFilesVisitor);

            toDelete.ToList().ForEach(fsi =>
            {
                operation(new FileSystemOperation
                {
                    Kind = OperationKind.Delete,
                    Operand = fsi
                });
            });

            toCopy.ToList().ForEach(fsi =>
            {
                operation(new FileSystemOperation
                {
                    Kind = OperationKind.Copy,
                    Operand = fsi
                });
            });
        }
    }
}
