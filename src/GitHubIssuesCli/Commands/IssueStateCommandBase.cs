using System;
using System.Threading.Tasks;
using GitHubIssuesCli.Exceptions;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli.Commands
{
    public abstract class IssueStateCommandBase : GitHubIssueCommandBase
    {
        private readonly IReporter _reporter;

        protected IssueStateCommandBase(IGitHubClient gitHubClient, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService, IReporter reporter) 
            : base(gitHubClient, gitHubRepositoryDiscoveryService)
        {
            _reporter = reporter;
        }

        protected async Task<int> UpdateIssueItemState(IConsole console, string issueReference, ItemState newState, string comment)
        {
            try
            {
                var (issue, repository) = await GetIssueAsync(issueReference);

                // Check if we're working with an already closed issue 
                if (issue.State == newState)
                {
                    _reporter.Warn($"Issue {repository.Owner.Login}/{repository.Name}#{issue.Number} is already {newState.ToString().ToLower()}. No action taken.");

                    return ReturnCodes.Error;
                }

                // Add an optional comment
                if (!string.IsNullOrEmpty(comment))
                {
                    await GitHubClient.Issue.Comment.Create(repository.Owner.Login, repository.Name, issue.Number, comment);
                }                
                
                // Close the issue
                await GitHubClient.Issue.Update(repository.Owner.Login, repository.Name, issue.Number, new IssueUpdate
                {
                    State = newState
                });

                if (newState == ItemState.Closed)
                    console.Write("Closed ");
                else
                    console.Write("Re-opened ");
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