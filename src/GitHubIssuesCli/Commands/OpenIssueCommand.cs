using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GitHubIssuesCli.Services;
using McMaster.Extensions.CommandLineUtils;
using Octokit;

namespace GitHubIssuesCli.Commands
{
    [Command(Description = "Opens a GitHub Issue", ThrowOnUnexpectedArgument = false)]
    public class OpenIssueCommand : IssueStateCommandBase
    {
        [Option(CommandOptionType.SingleValue, Description = "An optional comment to add when re-opening the issue.")]    
        public string Comment { get; set; }
        
        [Argument(0, Description = "The reference to the issue to open (in the format owner/repo#123). " +
                                   "When running command from a directory containing a repository, only the issue number can be used.")]
        [Required]
        [RegularExpression("^((?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)\\#)?(?<issue>\\d+)$", 
            ErrorMessage = "The {0} argument should be in the format owner/repo#number or you can simply pass the issue number when inside a directory containing a GitHub repository")]
        public string Issue { get; set; }

        public OpenIssueCommand(IGitHubClient gitHubClient, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService, IReporter reporter) 
            : base(gitHubClient, gitHubRepositoryDiscoveryService, reporter)
        {
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            return await UpdateIssueItemState(console, Issue, ItemState.Open, Comment);
        }
    }
}