﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli
{
    [Command(Description = "List GitHub Issues")]
    internal class ListIssuesCommand : RequiresTokenCommandBase
    {
        public async Task<int> OnExecuteAsync(IConsole console, CommandLineApplication context)
        {
            IReadOnlyList<Issue> issues = null;
            
            var gitHubClient = new GitHubClient(new ProductHeaderValue("GitHub-Issues-CLI")) {Credentials = new Credentials(GitHubToken)};

            // Grab the currenbt user
            var user = await gitHubClient.User.Current();

            // Check if we are in a git repo, and if so try and get the info for the remote GH repo.
            // In this instance we will limit issues to this repository.
            var githubRepo = GitHubRepositoryInfo.Discover(System.Environment.CurrentDirectory);

            if (githubRepo != null)
            {
                // BUG: We need to filter by the current user
                issues = await gitHubClient.Issue.GetAllForRepository(githubRepo.User, githubRepo.Repository);
            }
            else
            {
                issues = await gitHubClient.Issue.GetAllForCurrent();
            }

            console.Write("Listing open issues assigned to ");
            console.Write($"@{user.Login}", ConsoleColor.DarkMagenta);
            if (githubRepo != null)
            {
                console.Write(" in ");
                console.Write($"{githubRepo.User}/{githubRepo.Repository}", ConsoleColor.DarkYellow);
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