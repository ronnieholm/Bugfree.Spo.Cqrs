using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;

// todo: call CreateFolderPath

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class AddFileToLibrary : Command
    {
        public AddFileToLibrary(ILogger l) : base(l) { }

        public Folder GetFolderRecursive(Folder folder, List<string> pathComponents)
        {
            // base case
            if (pathComponents.Count == 0)
            {
                return folder;
            }

            // inductive case
            var head = pathComponents.First();
            var ctx = folder.Context;
            Folder nextFolder;

            try
            {
                nextFolder = folder.Folders.GetByUrl(head);
                ctx.Load(folder, f => f.Folders);
                ctx.ExecuteQuery();

                pathComponents.RemoveAt(0);
                return GetFolderRecursive(nextFolder, pathComponents);
            }
            catch (ServerException e)
            {
                if (e.ServerErrorTypeName == "System.IO.DirectoryNotFoundException")
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public void Execute(ClientContext ctx, byte[] content, string library, string filePath, Action<File> setAdditionalProperties = null)
        {
            Logger.Verbose($"Started executing {nameof(AddFileToLibrary)} for file '{filePath}' on library '{library}'");

            var web = ctx.Web;
            var l = web.Lists.GetByTitle(library);
            ctx.Load(l, lst => lst.RootFolder.ServerRelativeUrl);
            ctx.ExecuteQuery();

            var candidate = web.GetFileByServerRelativeUrl(l.RootFolder.ServerRelativeUrl + "/" + filePath);
            ctx.Load(candidate, f => f.Exists, f => f.ServerRelativeUrl);
            ctx.ExecuteQuery();

            if (candidate.Exists)
            {
                Logger.Warning($"File '{filePath}' already in library '{library}'");
                return;
            }

            var folderPath = filePath.Split(new[] { '/' }).ToList();
            var filename = folderPath.Last();
            folderPath.RemoveAt(folderPath.Count() - 1);

            var candidateFolder = GetFolderRecursive(l.RootFolder, folderPath);
            if (candidateFolder == null)
            {
                Logger.Warning("Containing folder for path not found");
                return;
            }

            var newFile =
                candidateFolder.Files.Add(
                    new FileCreationInformation
                    {
                        Url = filename,
                        Content = content
                    });
            ctx.Load(newFile);
            ctx.ExecuteQuery();

            if (setAdditionalProperties != null)
            {
                setAdditionalProperties(newFile);
                ctx.ExecuteQuery();
            }
        }
    }
}
