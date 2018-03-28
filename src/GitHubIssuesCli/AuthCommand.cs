using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    [Command(Description = "Authenticates a user with GitHub")]
    class AuthCommand: CommandBase
    {
        [Required]
        [Argument(0,  Description = "The GitHub Personal Access Token to use")]
        public string Token { get; }

        private Task<int> OnExecuteAsync(IConsole console)
        {
            // For OAuth 2.0 flow from a console app,
            // see https://github.com/googlesamples/oauth-apps-for-windows/blob/master/OAuthConsoleApp/OAuthConsoleApp/Program.cs

            SettingsStore store = new SettingsStore();
            store.Set("GitHubAccessToken", Token);
            store.Save();

            console.WriteLine("GitHub Access Token saved!");
            
            return Task.FromResult(0);
        }
    }
}