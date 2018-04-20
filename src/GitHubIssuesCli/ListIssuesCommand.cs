using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
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
        
        [Option(CommandOptionType.SingleValue, Description = "The repository to limit the issues to", LongName = "repo")]
        [RegularExpression("^(?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)$", ErrorMessage = "The option {0} must be in the format owner/repo")]
        public string Repository { get; set; }
        
        [Option(CommandOptionType.SingleValue, Description = "The user who the issues are related to")]
        public string User { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The relation of the issues to the user", ShortName = "R", LongName = "rel")]
        public IssueRelation Relation { get; set; } = IssueRelation.Assigned;
        
        [Option(CommandOptionType.SingleValue, Description = "The state of the issues")]
        public ItemStateFilter State { get; set; } = ItemStateFilter.Open;
        
        public ListIssuesCommand(IGitHubClient gitHubClient, IFileSystem fileSystem, IReporter reporter) : base(gitHubClient, fileSystem)
        {
            _reporter = reporter;
        }

        public async Task<int> OnExecuteAsync(IConsole console, CommandLineApplication context)
        {
            IReadOnlyList<Issue> issues = null;
            
            // See if a repo was passed in
            if (!string.IsNullOrEmpty(Repository))
            {
                try
                {
                    var match = Regex.Match(Repository, "^(?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)$");
                    var repositoryInfo = await  GitHubClient.Repository.Get(match.Groups["owner"].Value, match.Groups["repo"].Value);

                    _criteria.Owner = repositoryInfo.Owner.Login;
                    _criteria.Repository = repositoryInfo.Name;
                }
                catch (NotFoundException)
                {
                    _reporter.Error($"'{Repository}' is not a valid GitHub repository");
                    return 1;
                }
            }
            
            // If we do not have a repo passed in, then try and determine repo from the current folder
            if (string.IsNullOrEmpty(_criteria.Owner) && string.IsNullOrEmpty(_criteria.Repository))
            {
                var repositoryInfo = await GetGitHubRepositoryFromFolder();
                
                _criteria.Owner = repositoryInfo.Owner.Login;
                _criteria.Repository = repositoryInfo.Name;
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
                    Assignee = Relation == IssueRelation.Assigned ? _criteria.User : null,
                    Creator = Relation == IssueRelation.Created ? _criteria.User : null,
                    Mentioned = Relation == IssueRelation.Mentioned ? _criteria.User : null,
                    State = State
                });
            }
            else if (_criteria.Owner!= null)
            {
                issues = await GitHubClient.Issue.GetAllForOrganization(_criteria.Owner, new IssueRequest
                {
                    Filter = GetIssueFilter(Relation),
                    State = State
                });
            }
            else
            {
                issues = await GitHubClient.Issue.GetAllForCurrent(new IssueRequest
                {
                    Filter = GetIssueFilter(Relation),
                    State = State
                });
            }
            
            console.Write("Listing ");
            console.Write($"{State}", ConsoleColor.Blue);
            console.Write(" issues ");
            switch (Relation)
            {
                case IssueRelation.Assigned:
                    console.Write("assigned to ");
                    break;
                case IssueRelation.Created:
                    console.Write("created by ");
                    break;
                case IssueRelation.Mentioned:
                    console.Write("mentioning ");
                    break;
            }
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
            if (!string.IsNullOrEmpty(_criteria.Repository))
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

        private IssueFilter GetIssueFilter(IssueRelation relation)
        {
            switch (relation)
            {
                case IssueRelation.Assigned:
                    return IssueFilter.Assigned;
                case IssueRelation.Created:
                    return IssueFilter.Created;
                case IssueRelation.Mentioned:
                    return IssueFilter.Mentioned;
                default:
                    return IssueFilter.Assigned;
            }
        }
    }
}