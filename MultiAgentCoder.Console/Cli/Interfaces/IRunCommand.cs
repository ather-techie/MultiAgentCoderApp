namespace MultiAgentCoder.Console.Cli.Interfaces
{
    public interface IRunCommand
    {
        Task<int> ExecuteAsync(string[] args);
    }
}