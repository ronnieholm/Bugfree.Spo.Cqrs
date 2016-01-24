namespace Bugfree.Spo.Cqrs.Core
{
    public class Command
    {
        protected ILogger Logger { get; private set; }

        protected Command(ILogger logger)
        {
            Logger = logger;
        }
    }
}
