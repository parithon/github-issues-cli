﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli
{
    [Command(Description = "List GitHub Issues")]
    internal class ListIssuesCommand : RequiresTokenCommandBase
    {
        private readonly IReporter _reporter;
        private readonly ListIssueCriteria _criteria = new ListIssueCriteria();
        
        [Option(CommandOptionType.SingleValue, Description = "The user whose issues...")]
        public string User { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The state of the issues")]
        public ItemStateFilter State { get; set; } = ItemStateFilter.Open;
        
        public ListIssuesCommand(IGitHubClient gitHubClient, IReporter reporter) : base(gitHubClient)
        {
            _reporter = reporter;
        }

        public async Task<int> OnExecuteAsync(IConsole console, CommandLineApplication context)
        {
            IReadOnlyList<Issue> issues = null;
            
            // Check if we are in a git repo, and if so try and get the info for the remote GH repo.
            // In this instance we will limit issues to this repository.
            var githubRepo = GitHubRepositoryInfo.Discover(System.Environment.CurrentDirectory);

            if (githubRepo != null)
            {
                // Check if we're working with a fork. If so, we want to grab issues from the parent
                var repositoryInfo = await GitHubClient.Repository.Get(githubRepo.User, githubRepo.Repository);
                if (repositoryInfo.Fork)
                {
                    _criteria.Owner = repositoryInfo.Parent.Owner.Login;
                    _criteria.Repository = repositoryInfo.Parent.Name;
                }
                else
                {
                    _criteria.Owner = repositoryInfo.Owner.Login;
                    _criteria.Repository = repositoryInfo.Name;
                }
            }
            
            // Validate the user
            var currentUserInfo = await GitHubClient.User.Current();            
            if (!string.IsNullOrEmpty(User))
            {
                try
                {
                    _criteria.User = (await GitHubClient.User.Get(User)).Login;
                }
                catch (NotFoundException e)
                {
                    _reporter.Error($"'{User}' is not a valid GitHub user");
                    return 1;
                }
            }
            else
            {
                // If now user was passed, we assume the current user
                _criteria.User = currentUserInfo.Login;
            }

            // If the user is not the current user, we need to ensure that a repo is also specified, as we can only
            // query across repositories for the current user
            if (string.IsNullOrEmpty(_criteria.Repository) && string.Compare(_criteria.User, currentUserInfo.Login, StringComparison.OrdinalIgnoreCase) != 0)
            {
                _reporter.Error("In order to filter issues by user, you need to specify a repository");
                return 1;
            }
            
            // Get the issues
            if (_criteria.Owner != null && _criteria.Repository != null)
            {
                issues = await GitHubClient.Issue.GetAllForRepository(_criteria.Owner, _criteria.Repository, new RepositoryIssueRequest
                {
                    Assignee = _criteria.User,
                    State = State
                });
            }
            else if (_criteria.Owner!= null)
            {
                issues = await GitHubClient.Issue.GetAllForOrganization(_criteria.Owner, new IssueRequest
                {
                    Filter = IssueFilter.Assigned,
                    State = State
                });
            }
            else
            {
                issues = await GitHubClient.Issue.GetAllForCurrent(new IssueRequest
                {
                    Filter = IssueFilter.Assigned,
                    State = State
                });
            }
            
            console.Write("Listing ");
            console.Write($"{State}", ConsoleColor.Blue);
            console.Write(" issues assigned to ");
            console.Write($"@{_criteria.User}", ConsoleColor.Blue);
            if (_criteria.Owner != null && _criteria.Repository != null)
            {
                console.Write(" in ");
                console.Write($"{_criteria.Owner}/{_criteria.Repository}", ConsoleColor.DarkYellow);
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

            console.Write(" ");
            foreach (var issueLabel in issue.Labels)
            {
                var labelColors = ConsoleColorHelper.FromHex(issueLabel.Color);
                console.BackgroundColor = labelColors.BackgroundCololr;
                console.ForegroundColor = labelColors.ForegroundColor;
                console.Write($"{issueLabel.Name}");
                console.ResetColor();
                
                console.Write(" ");    
            }
            
            console.WriteLine();
        }
    }
}