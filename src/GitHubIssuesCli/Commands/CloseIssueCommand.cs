using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GitHubIssuesCli.Exceptions;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli.Commands
{
    [Command(Description = "Closes a GitHub Issue", ThrowOnUnexpectedArgument = false)]
    public class CloseIssueCommand : GitHubIssueCommandBase
    {
        private readonly IReporter _reporter;
    
        [Option(CommandOptionType.SingleValue, Description = "An optional comment to add when closing the issue.")]    
        public string Comment { get; set; }
        
        [Argument(0, Description = "The reference to the issue to close (in the format owner/repo#123). " +
                                   "When running command from a directory containing a repository, only the issue number can be used.")]
        [Required]
        [RegularExpression("^((?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)\\#)?(?<issue>\\d+)$", 
            ErrorMessage = "The {0} argument should be in the format owner/repo#number or you can simply pass the issue number when inside a directory containing a GitHub repository")]
        public string Issue { get; set; }

        public CloseIssueCommand(IGitHubClient gitHubClient, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService, IReporter reporter) 
            : base(gitHubClient, gitHubRepositoryDiscoveryService)
        {
            _reporter = reporter;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            try
            {
                var (issue, repository) = await GetIssueAsync(Issue);

                // Check if we're working with an already closed issue 
                if (issue.State == ItemState.Closed)
                {
                    _reporter.Warn($"Issue {repository.Owner.Login}/{repository.Name}#{issue.Number} is already closed. No action taken.");

                    return ReturnCodes.Error;
                }

                // Add an optional comment
                if (!string.IsNullOrEmpty(Comment))
                {
                    await GitHubClient.Issue.Comment.Create(repository.Owner.Login, repository.Name, issue.Number, Comment);
                }                
                
                // Close the issue
                await GitHubClient.Issue.Update(repository.Owner.Login, repository.Name, issue.Number, new IssueUpdate
                {
                    State = ItemState.Closed
                });
                
                console.Write("Closed ");
                console.Write($"{repository.Owner.Login}/{repository.Name}#{issue.Number}", ConsoleColor.Yellow);
                console.WriteLine();
            }
            catch (CommandValidationException e)
            {
                _reporter.Error(e.Message);

                return ReturnCodes.Error;
            }

            return ReturnCodes.Ok;
        }
    }
}