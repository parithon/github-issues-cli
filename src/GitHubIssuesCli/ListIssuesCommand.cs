using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    [Command(Description = "List GitHub Issues")]
    internal class ListIssuesCommand : RequiresTokenCommandBase
    {
        Task<int> OnExecuteAsync(IConsole console, CommandLineApplication context)
        {
            console.WriteLine($"GitHub token passed is {GitHubToken}");

            return Task.FromResult(0);
        }


    }
}