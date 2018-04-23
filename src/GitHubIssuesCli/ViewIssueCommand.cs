using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileSystemGlobbing;
using Octokit;

namespace GitHubIssuesCli
{
    internal class ViewIssueCommand : GitHubCommandBase
    {
        private readonly IReporter _reporter;

        [Argument(0, Description = "The reference to the issue to open")]
        [Required]
        [RegularExpression("^((?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)\\#)?(?<issue>\\d+)$", 
            ErrorMessage = "The {0} argument should be in the format owner/repo#number or you can simply pass the issue number when inside a directory containing a GitHub repository")]
        public string Issue { get; set; }

        public ViewIssueCommand(IGitHubClient gitHubClient, IFileSystem fileSystem, IGitHubRepositoryDiscoveryService gitHubRepositoryDiscoveryService, IReporter reporter) 
            : base(gitHubClient, gitHubRepositoryDiscoveryService, fileSystem)
        {
            _reporter = reporter;
        }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            Repository repositoryInfo = null;

            // Check to see whether the Issue argument contains thte owner and the repo
            var match = Regex.Match(Issue, "^((?<owner>[\\w-.]+)\\/(?<repo>[\\w-.]+)\\#)?(?<issue>\\d+)$");
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
                    _reporter.Error($"'{owner}/{repo}' is not a valid GitHub repository");
                    return 1;
                }
            }
            else
            {
                repositoryInfo = await GetGitHubRepositoryFromFolder();
            }

            // If we are unable to determine the repo, then return an error
            if (repositoryInfo == null)
            {
                _reporter.Error("No repository specified. You need to either specify the issue number in the format owner/repo#number, or alternatively you need to run the command from a directory containing a GitHub repository");
                
                return 1;
            }
            
            // Get the issue from the repo
            int issueNumber = Convert.ToInt32(match.Groups["issue"].Value);
            try
            {
                // Get the actual issue
                var issue = await GitHubClient.Issue.Get(repositoryInfo.Owner.Login, repositoryInfo.Name, issueNumber);
                
                // Open the issue in the browser
                OpenBrowser(issue.HtmlUrl);
                
            }
            catch (NotFoundException)
            {
                _reporter.Error($"Issue #{issueNumber} not found in repository {repositoryInfo.Owner.Login}/{repositoryInfo.Name}");
                return 1;
            }

            return 0;
        }
        
        /// <remarks>
        /// Shamelessly stolen from https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/blob/79a7afe6e45027b2ee14206a653436e5853f6b81/NetCoreConsoleClient/src/NetCoreConsoleClient/SystemBrowser.cs#L71-L97
        /// </remarks>
        private static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}