using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli
{
    [Command(Description = "List GitHub Issues")]
    internal class ListIssuesCommand : RequiresTokenCommandBase
    {
        public ListIssuesCommand(IGitHubClient gitHubClient) : base(gitHubClient)
        {
        }

        public async Task<int> OnExecuteAsync(IConsole console, CommandLineApplication context)
        {
            ListIssueCriteria criteria = new ListIssueCriteria();
            IReadOnlyList<Issue> issues = null;
            
            // Grab the current user
            criteria.Assigned = (await GitHubClient.User.Current()).Login;

            // Check if we are in a git repo, and if so try and get the info for the remote GH repo.
            // In this instance we will limit issues to this repository.
            var githubRepo = GitHubRepositoryInfo.Discover(System.Environment.CurrentDirectory);

            if (githubRepo != null)
            {
                // Check if we're working with a fork. If so, we want to grab issues from the parent
                var repositoryInfo = await GitHubClient.Repository.Get(githubRepo.User, githubRepo.Repository);
                if (repositoryInfo.Fork)
                {
                    criteria.Owner = repositoryInfo.Parent.Owner.Login;
                    criteria.Repository = repositoryInfo.Parent.Name;
                }
                else
                {
                    criteria.Owner = repositoryInfo.Owner.Login;
                    criteria.Repository = repositoryInfo.Name;
                }
            }

            if (criteria.Owner != null && criteria.Repository != null)
            {
                issues = await GitHubClient.Issue.GetAllForRepository(criteria.Owner, criteria.Repository, new RepositoryIssueRequest
                {
                    Assignee = criteria.Assigned
                });
            }
            else if (criteria.Owner!= null)
            {
                issues = await GitHubClient.Issue.GetAllForOrganization(criteria.Owner, new IssueRequest
                {
                    Filter = IssueFilter.Assigned
                });
            }
            else
            {
                issues = await GitHubClient.Issue.GetAllForCurrent(new IssueRequest
                {
                    Filter = IssueFilter.Assigned
                });
            }

            console.Write("Listing open issues assigned to ");
            console.Write($"@{criteria.Assigned}", ConsoleColor.DarkMagenta);
            if (criteria.Owner != null && criteria.Repository != null)
            {
                console.Write(" in ");
                console.Write($"{criteria.Owner}/{criteria.Repository}", ConsoleColor.DarkYellow);
            }            
            console.WriteLine();
            console.WriteLine();

            if (issues.Count == 0)
            {
                console.WriteEmptyResult("No issues found");
                Console.WriteLine();

                return 0;
            }
            
            // Issues for specific repo we display non-grouped
            if (githubRepo != null)
                DisplayNonGrouped(issues, console);
            else
                DisplayGrouped(issues, console);
                    

            return 0;
        }

        private void DisplayNonGrouped(IReadOnlyList<Issue> issues, IConsole console)
        {
            foreach (var issue in issues)
                DisplayIssue(issue, console, 0);
        }

        private void DisplayGrouped(IReadOnlyList<Issue> issues, IConsole console)
        {
            var groupedIssues = issues.GroupBy(i => i.Repository.FullName)
                .OrderBy(g => g.Key);

            foreach (var repo in groupedIssues)
            {
                console.WriteHeader(repo.Key);

                foreach (var issue in repo)
                    DisplayIssue(issue, console, 1);

                console.WriteLine();
            }
        }

        private static void DisplayIssue(Issue issue, IConsole console, int indentLevel)
        {
            console.WriteIndent(indentLevel);
            console.Write($"#{issue.Number} ", ConsoleColor.DarkGreen);
            console.Write(issue.Title);
            console.Write($" @{issue.User.Login}", ConsoleColor.DarkMagenta);
            console.WriteLine();
        }
    }
}