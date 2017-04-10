using System;
using System.Linq;
using Xunit;
using Microsoft.SharePoint.Client;
using Bugfree.Spo.Cqrs.Core.Commands;

namespace Bugfree.Spo.Cqrs.Core.Tests 
{
    public class Remove_list_command : EmptyWebBase 
    {
        [Fact]
        void Removes_if_exists() 
        {
            var title = Guid.NewGuid().ToString();
            new CreateListFromTemplate(Logger).Execute(Context, ListTemplateType.GenericList, title);
            new RemoveList(Logger).Execute(Context, title);

            var lists = Context.Web.Lists;
            Context.Load(lists);
            Context.ExecuteQuery();
            Assert.Equal(0, lists.Count(l => l.Title == title));
        }

        [Fact]
        void No_change_if_not_exists() 
        {
            var lists = Context.Web.Lists;
            Context.Load(lists);
            Context.ExecuteQuery();
            var a = lists.Count();
            new RemoveList(Logger).Execute(Context, Guid.NewGuid().ToString());

            Context.Load(lists);
            Context.ExecuteQuery();
            var b = lists.Count();
            Assert.Equal(a, b);
        }
    }
}
