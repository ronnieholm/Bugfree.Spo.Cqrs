using System;
using System.Linq;
using System.Security;
using Microsoft.SharePoint.Client;
using Bugfree.Spo.Cqrs.Core.Commands;

namespace Bugfree.Spo.Cqrs.Core.Tests 
{
    public class EmptyWebBase : IDisposable
    {
        public string TestId { get; set; }
        public ILogger Logger { get; set; }
        public ClientContext Parent { get; set; }
        public ClientContext Context { get; set; }

        // todo: move to config file
        private const string Username = "xxx@yyy.onmicrosoft.com";
        private const string Password = "zzz";

        private ClientContext SetupClientContext(string url) 
        {
            var securePassword = new SecureString();
            Password.ToCharArray().ToList().ForEach(securePassword.AppendChar);
            return new ClientContext(url) 
            {
                Credentials = new SharePointOnlineCredentials(Username, securePassword)
            };
        }

        public EmptyWebBase() 
        {
            Logger = new ColoredConsoleLogger();

            Parent = SetupClientContext("https://xxx.sharepoint.com/teams/yyy");
            var webTitle = Guid.NewGuid().ToString();
            new CreateWeb(Logger).Execute(Parent, webTitle, webTitle, "STS#0", true, 1033);
            Context = SetupClientContext($"{Parent.Url}/{webTitle}");
        }

        public void Dispose() 
        {
            if (Parent != null) 
            {
                Parent.Dispose();
            }

            if (Context != null) 
            {
                Context.Dispose();
            }
        }
    }
}
