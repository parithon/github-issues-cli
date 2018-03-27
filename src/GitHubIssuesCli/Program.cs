using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace GitHubIssuesCli
{
    [Command(Name = "ghi", Description = "A GitHub Issues client")]
    [Subcommand("auth", typeof(AuthCommand))]
    [HelpOption]
    class Program
    {
        static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

        private Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            app.ShowHelp();
            
            return Task.FromResult(0);
        }
    }

    [Command(Description = "Authenticates a user with GitHub")]
    [HelpOption]
    class AuthCommand
    {
        [Argument(0,  Description = "The GitHub Personal Access Token to use")]
        public string Token { get; }

        private Task<int> OnExecuteAsync(IConsole console)
        {
            // See https://github.com/googlesamples/oauth-apps-for-windows/blob/master/OAuthConsoleApp/OAuthConsoleApp/Program.cs
            
            return Task.FromResult(0);
        }
    }
}

