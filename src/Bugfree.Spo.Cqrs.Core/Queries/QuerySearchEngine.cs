﻿using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Client.Search.Query;
using System.Linq;

namespace Bugfree.Spo.Cqrs.Core.Queries
{
    public class QuerySearchEngine : Query
    {
        private const int BatchSize = 500;

        public QuerySearchEngine(ILogger l) : base(l) { }

        private void QuerySearchEngineRecursive(ClientContext ctx, string kql, Action<IDictionary<string, object>> procesResultRow, int startRow)
        {
            Logger.Verbose($"Fetching results in range {startRow} to {startRow + BatchSize + 1}");

            var executor = new SearchExecutor(ctx);
            var results = executor.ExecuteQuery(
                new KeywordQuery(ctx)
                {
                    QueryText = kql,
                    StartRow = startRow,
                    RowLimit = BatchSize
                });
            ctx.ExecuteQuery();

            var rows = results.Value[0];
            Logger.Verbose($"Result contains {rows.RowCount} rows");
            rows.ResultRows.ToList().ForEach(procesResultRow);

            if (rows.RowCount > 0)
            {
                QuerySearchEngineRecursive(ctx, kql, procesResultRow, startRow + BatchSize);
            }
        }

        public void Execute(ClientContext ctx, string kql, Action<IDictionary<string, object>> procesRow)
        {
            Logger.Verbose($"About to execute {nameof(QuerySearchEngine)} with query '{kql}'");
            QuerySearchEngineRecursive(ctx, kql, procesRow, 0);
        }
    }
}