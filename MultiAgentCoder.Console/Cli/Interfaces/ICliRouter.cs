namespace MultiAgentCoder.Console.Cli.Interfaces
{
    public interface ICliRouter
    {
        Task RouteAsync(string[] args);
    }
}