using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    internal abstract class RequiresTokenCommandBase : CommandBase
    {
        protected string GitHubAccessToken { get; private set; }

        protected virtual Task<int> OnExecuteAsync(IConsole console)
        {
            SettingsStore store = new SettingsStore();
            if (!store.ContainsKey("GitHubAccessToken"))
            {
                console.Error.WriteLine("You must authenticate with GitHub first");
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }
    }
}