﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GitHubIssuesCli.Exceptions;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli.Commands
{
    [Command(Description = "Closes a GitHub Issue", ThrowOnUnexpectedArgument = false)]
    public class CloseIssueCommand : IssueStateCommandBase
    {
        [Option(CommandOptionType.SingleValue, Description = "An optional comment to add when closing the issue.")]    
        public string Comment { get; set; }
        
        [Argument(0, Description = "The reference to the issue to close (in the format owner/repo#123). " +
                                   "When running command from a directory containing a repository, only the issue number can be used.")]
        [Required]
        [RegularExpression("^((?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)\\#)?(?<issue>\\d+)$", 
            ErrorMessage = "The {0} argument should be in the format owner/repo#number or you can simply pass the issue number when inside a directory containing a GitHub repository")]
        public string Issue { get; set; }

        public CloseIssueCommand(IGitHubClient gitHubClient, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService, IReporter reporter) 
            : base(gitHubClient, gitHubRepositoryDiscoveryService, reporter)
        {
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            return await UpdateIssueItemState(console, Issue, ItemState.Closed, Comment);
        }
    }
}