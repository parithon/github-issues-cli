using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    [Command(Description = "List GitHub Issues")]
    internal class ListIssuesCommand : RequiresTokenCommandBase
    {
        protected override Task<int> OnExecuteAsync(IConsole console)
        {
            return base.OnExecuteAsync(console);
        }
    }
}