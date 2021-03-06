﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GitHubIssuesCli.Exceptions;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli.Commands
{
    [Command(Description = "View details of specific GitHub Issue", ThrowOnUnexpectedArgument = false)]
    public class ViewIssueCommand : GitHubIssueCommandBase
    {
        private readonly IBrowserService _browserService;
        private readonly IReporter _reporter;

        [Option(CommandOptionType.NoValue, Description = "Opens the issue in the web browser.")]
        public bool Browser { get; set; }
        
        [Argument(0, Description = "The reference to the issue to view (in the format owner/repo#123). " +
                                   "When running command from a directory containing a repository, only the issue number can be used.")]
        [Required]
        [RegularExpression("^((?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)\\#)?(?<issue>\\d+)$", 
            ErrorMessage = "The {0} argument should be in the format owner/repo#number or you can simply pass the issue number when inside a directory containing a GitHub repository")]
        public string Issue { get; set; }
        
        public ViewIssueCommand(IGitHubClient gitHubClient, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService, 
            IBrowserService browserService, IReporter reporter) 
            : base(gitHubClient, gitHubRepositoryDiscoveryService)
        {
            _browserService = browserService;
            _reporter = reporter;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            try
            {
                var (issue, repositoryInfo) = await GetIssueAsync(Issue);

                // Open the issue in the browser
                if (Browser)
                    _browserService.OpenBrowser(issue.HtmlUrl);
                else
                {
                    console.Write(issue.Title);
                    console.Write($" ({repositoryInfo.Owner.Login}/{repositoryInfo.Name}#{issue.Number})", ConsoleColor.Yellow);
                    console.WriteLine();
                    console.WriteLine();

                    console.WriteLine(issue.Body);
                    console.WriteLine();

                    if (issue.Labels != null && issue.Labels.Count > 0)
                    {
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
                        console.WriteLine();
                    }

                    console.Write("Opened by: ");
                    console.Write(issue.User.Login, ConsoleColor.Blue);
                    console.WriteLine();

                    console.Write("Status: ");
                    console.Write($"{issue.State.Value}", ConsoleColor.Blue);
                    if (issue.State.Value == ItemState.Closed && issue.ClosedAt.HasValue)
                    {
                        console.Write(" by ");
                        console.Write(issue.ClosedBy.Login, ConsoleColor.Blue);
                        console.Write(" on ");
                        console.Write(issue.ClosedAt.Value.ToString("d"), ConsoleColor.Blue);
                    }
                    console.WriteLine();

                    if (issue.State.Value == ItemState.Open)
                    {
                        console.Write("Assigned to: ");
                        if (issue.Assignees == null || issue.Assignees.Count == 0)
                        {
                            console.Write("UNASSIGNED", ConsoleColor.Blue);
                        }
                        else
                        {
                            console.Write(string.Join(", ", issue.Assignees.Select(a => a.Login)), ConsoleColor.Blue);
                        }                        
                        console.WriteLine();
                    }
                }
            }
            catch (CommandValidationException ex)
            {
                _reporter.Error(ex.Message);
                
                return 1;
            }

            return 0;
        }
    }
}