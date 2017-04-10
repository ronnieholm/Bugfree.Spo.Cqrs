using System;
using System.Linq;
using Xunit;
using Bugfree.Spo.Cqrs.Core.Commands;

namespace Bugfree.Spo.Cqrs.Core.Tests {
    public class Create_web_command : EmptyWebBase
    {
        [Fact]
        void Creates_if_not_exists() 
        {
            var title = Guid.NewGuid().ToString();
            var url = Guid.NewGuid().ToString();

            var webs = Context.Web.Webs;
            Context.Load(webs);
            Context.ExecuteQuery();
            var preCount = webs.Count();

            new CreateWeb(Logger).Execute(Context, title, url, "STS#0", true, 1033);

            Context.Load(webs);
            Context.ExecuteQuery();
            var web = webs.Single(w => w.Url.EndsWith(url));
            var postCount = webs.Count();
            
            Assert.Equal(preCount + 1, postCount);
            Assert.Equal(title, web.Title);
        }

        [Fact]
        void No_change_if_exists() 
        {
            var title = Guid.NewGuid().ToString();
            var url = title;

            var webs = Context.Web.Webs;
            Context.Load(webs);
            Context.ExecuteQuery();
            var preCount = webs.Count();

            new CreateWeb(Logger).Execute(Context, title, url, "STS#0", true, 1033);
            new CreateWeb(Logger).Execute(Context, title, url, "STS#0", true, 1033);

            Context.Load(webs);
            Context.ExecuteQuery();
            var postCount = webs.Count();

            Assert.Equal(preCount + 1, postCount);
        }

        [Fact]
        void Apply_action_lambda() 
        {
            // change description            
        }
    }
}
