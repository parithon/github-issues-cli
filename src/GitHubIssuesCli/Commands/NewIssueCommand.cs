using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli.Commands
{
    [Command(Description = "Creates a new GitHub Issue", ThrowOnUnexpectedArgument = false)]
    public class NewIssueCommand : GitHubCommandBase
    {
        private readonly IReporter _reporter;

        [Option(CommandOptionType.SingleValue, Description = "Body of the issue")]
        public string Body { get; set; }
        
        [Required]
        [Argument(0, "Title of the issue")]
        public string Title { get; set; }
        
        [Option(CommandOptionType.SingleValue, 
            Description = "The repository in which to create the new issue. By default the repository in current folder will be used.", 
            LongName = "repo")]
        [RegularExpression("^(?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)$", ErrorMessage = "The option {0} must be in the format owner/repo")]
        public string Repository { get; set; }
        
        public NewIssueCommand(IGitHubClient gitHubClient, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService, IReporter reporter) 
            : base(gitHubClient, gitHubRepositoryDiscoveryService)
        {
            _reporter = reporter;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            Repository repositoryInfo = null;

            // Check to see whether the Issue argument contains thte owner and the repo
            if (!string.IsNullOrEmpty(Repository))
            {
                var match = Regex.Match(Repository, "^(?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)$");
                if (match.Groups["owner"].Success && match.Groups["repo"].Success)
                {
                    string owner = match.Groups["owner"].Value;
                    string repo = match.Groups["repo"].Value;

                    try
                    {
                        repositoryInfo = await GitHubClient.Repository.Get(owner, repo);
                    }
                    catch (NotFoundException)
                    {
                        _reporter.Error($"'{owner}/{repo}' is not a valid GitHub repository");
                        return 1;
                    }
                }
            }
            else
            {
                repositoryInfo = await GetGitHubRepositoryFromFolder();
            }

            
            // If we are unable to determine the repo, then return an error
            if (repositoryInfo == null)
            {
                _reporter.Error("No repository specified. You need to either specify the issue number in the format owner/repo#number, or alternatively you need to run the command from a directory containing a GitHub repository");
                
                return 1;
            }
            
            // Create the issue
            var issue = await GitHubClient.Issue.Create(repositoryInfo.Owner.Login, repositoryInfo.Name, new NewIssue(Title)
            {
                Body = Body
            });

            // Display the issue number
            console.Write("Created ");
            console.Write($"{repositoryInfo.Owner.Login}/{repositoryInfo.Name}#{issue.Number}", ConsoleColor.Yellow);
            console.WriteLine();
            
            return 0;
        }
    }
}