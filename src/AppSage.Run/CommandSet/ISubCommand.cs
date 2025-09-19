using System.CommandLine;

namespace AppSage.Run.CommandSet
{
    public interface ISubCommand
    {
        string Name { get; }
        string Description { get; }
        Command Build();

    }

    public interface ISubCommand<TOptions> : ISubCommand
    {
        int Execute(TOptions options);
    }

    public interface  ISubCommandWithNoOptions:ISubCommand
    {
        int Execute();
    }
}
