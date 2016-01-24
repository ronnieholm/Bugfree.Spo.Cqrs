namespace Bugfree.Spo.Cqrs.Core
{
    public class Query
    {
        protected ILogger Logger { get; private set; }

        protected Query(ILogger logger)
        {
            Logger = logger;
        }
    }
}
