using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    [Command(Description = "List GitHub Issues")]
    internal class ListIssuesCommand : RequiresTokenCommandBase
    {
        Task<int> OnExecuteAsync(IConsole console)
        {
            console.WriteLine("This is the list of issues");

            return Task.FromResult(0);
        }
    }
}