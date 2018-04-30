using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitHubIssuesCli.Exceptions;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli.Commands
{
    public abstract class GitHubIssueCommandBase : GitHubCommandBase
    {
        protected GitHubIssueCommandBase(IGitHubClient gitHubClient, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService) 
            : base(gitHubClient, gitHubRepositoryDiscoveryService)
        {
        }

        protected async Task<(Issue, Repository)> GetIssueAsync(string issueReference)
        {
            Repository repositoryInfo = null;

            // Check to see whether the Issue argument contains thte owner and the repo
            var match = Regex.Match(issueReference, "^((?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)\\#)?(?<issue>\\d+)$");
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
                    // If the repo is invalid, throw an error
                    throw new CommandValidationException($"'{owner}/{repo}' is not a valid GitHub repository");
                }
            }
            else
            {
                repositoryInfo = await GetGitHubRepositoryFromFolder();
            }

            // If we are unable to determine the repo, then throw an error
            if (repositoryInfo == null)
            {
                throw new CommandValidationException("No repository specified. You need to either specify the issue number in the format owner/repo#number, or alternatively you need to run the command from a directory containing a GitHub repository");
            }
            
            // Get the issue from the repo
            int issueNumber = Convert.ToInt32(match.Groups["issue"].Value);
            try
            {
                var issue = await GitHubClient.Issue.Get(repositoryInfo.Owner.Login, repositoryInfo.Name, issueNumber);

                return (issue, repositoryInfo);
            }
            catch (NotFoundException)
            {
                // If the issue could not be found, throw an error
                throw new CommandValidationException($"Issue #{issueNumber} not found in repository {repositoryInfo.Owner.Login}/{repositoryInfo.Name}");
            }
        } 
    }
}