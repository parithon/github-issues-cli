using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli
{
    internal abstract class RequiresTokenCommandBase : CommandBase
    {
        [Option(CommandOptionType.SingleValue, Description = "Your GitHub Personal Access token")]
        public string Token { get; set;  }

        protected IGitHubClient GitHubClient { get; }

        protected RequiresTokenCommandBase(IGitHubClient gitHubClient)
        {
            GitHubClient = gitHubClient;
        }
        
        internal ValidationResult OnValidate(ValidationContext context)
        {
            // Ensure we have a token either as an option or as an environment variable
            if (string.IsNullOrEmpty(Token) && Environment.GetEnvironmentVariable(Constants.GitHubTokenEnvironmentVariable) == null)
            {
                return new ValidationResult("You need to specify a GitHub token");
            }

            // Set the token on the GH client
            string token = string.IsNullOrEmpty(Token)
                ? Environment.GetEnvironmentVariable(Constants.GitHubTokenEnvironmentVariable)
                : Token;
            GitHubClient.Connection.Credentials = new Credentials(token);

            // Validation is A-OK
            return ValidationResult.Success;
        }

        protected async Task<Repository> GetGitHubRepositoryFromFolder(string folder)
        {
            var githubRepo = GitHubRepositoryInfo.Discover(System.Environment.CurrentDirectory);
            if (githubRepo != null)
            {
                // Check if we're working with a fork. If so, we want to grab issues from the parent
                var repositoryInfo = await GitHubClient.Repository.Get(githubRepo.User, githubRepo.Repository);
                if (repositoryInfo.Fork)
                    return repositoryInfo.Parent;

                return repositoryInfo;
            }

            return null;
        }
    }
}