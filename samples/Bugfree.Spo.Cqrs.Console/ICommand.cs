namespace Bugfree.Spo.Cqrs.Console
{
    public enum ExitCode
    {
        Success = 0,
        Failure
    };

    public interface ICommand
    {
        string Usage { get; }
        string Description { get; }
        ExitCode Execute(string[] args);
    }
}
