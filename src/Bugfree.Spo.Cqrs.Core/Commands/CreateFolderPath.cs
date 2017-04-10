using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Client;

namespace Bugfree.Spo.Cqrs.Core.Commands
{
    public class CreateFolderPath : Command
    {
        public CreateFolderPath(ILogger l) : base(l) { }

        private Folder CreateFolderPathRecursive(Folder f, List<string> pathComponents)
        {
            // base case
            if (pathComponents.Count == 0) return f;
            var head = pathComponents.First();

            // inductive case
            Folder nextFolder;
            try
            {
                nextFolder = f.Folders.GetByUrl(head);
                f.Context.Load(nextFolder);
                f.Context.ExecuteQuery();
            }
            catch (ServerException e) when (e.ServerErrorTypeName == "System.IO.DirectoryNotFoundException")
            {
                nextFolder = f.Folders.Add(head);
                nextFolder.Update();
                f.Context.ExecuteQuery();
                Logger.Verbose($"Folder '{head}' created");
            }

            pathComponents.RemoveAt(0);
            return CreateFolderPathRecursive(nextFolder, pathComponents);
        }

        public void Execute(ClientContext ctx, string libraryName, string path)
        {
            Logger.Verbose($"Started executing {nameof(CreateFolderPath)} for library '{libraryName}' with path '{path}'");

            var library = ctx.Web.Lists.GetByTitle(libraryName);
            var root = library.RootFolder;
            ctx.Load(library);
            ctx.Load(root);
            ctx.ExecuteQuery();

            CreateFolderPathRecursive(root, path.Split(new[] { '/' }).ToList());
        }
    }
}
