namespace Bugfree.Spo.Cqrs.Console
{
    class FixWrongSomething : ICommand
    {
        public string Description
        {
            get
            {
                return "";
            }
        }

        public string Usage
        {
            get
            {
                return "";
            }
        }

        public ExitCode Execute(string[] args)
        {
            return ExitCode.Success;
        }
    }
}
